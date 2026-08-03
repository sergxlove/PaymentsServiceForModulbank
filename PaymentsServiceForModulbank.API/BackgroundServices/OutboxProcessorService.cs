using PaymentsServiceForModulbank.API.Responses;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite;
using PaymentsServiceForModulebank.Sqlite.Abstractions;
using System.Text;
using System.Text.Json;

namespace PaymentsServiceForModulbank.API.BackgroundServices
{
    public class OutboxProcessorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        public OutboxProcessorService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessOutboxMessages(stoppingToken);
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PaymengsServiceDbContext>();
            var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var operationService = scope.ServiceProvider.GetRequiredService<IOperationsRepository>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventsRepository>();
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("ProviderClient");
            try
            {
                List<OutboxMessages> mes = await outboxService.GetByStatusAsync(new List<string>
                { "PENDING", "PROCESSING" }, 10, cancellationToken);
                if (!mes.Any())
                    return;
                foreach(OutboxMessages m in mes)
                {
                    await ProcessSingleMessage(m, httpClient, db, outboxService, operationService, eventService,
                        cancellationToken);
                }
            }
            catch { }
        }

        private async Task ProcessSingleMessage(OutboxMessages message, HttpClient httpClient,
            PaymengsServiceDbContext context, IOutboxRepository outboxRepository, 
            IOperationsRepository operationsRepository, IEventsRepository eventsRepository,
            CancellationToken token)
        {
            await using var transaction = await context.Database.BeginTransactionAsync(token);
            try
            {
                int resultUpdate = await outboxRepository.UpdateStatusAsync(message.Id, "PROCESSING", token);
                if (resultUpdate == 0)
                    return;
                var payload = JsonSerializer.Deserialize<Operations>(message.Payload);
                var requestContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8,
                    "application/json");
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://provider-simulator:8081/payments")
                {
                    Content = requestContent
                };
                requestMessage.Headers.Add("Idempotency-Key", payload!.OperationId);
                requestMessage.Headers.Add("X-Correlation-ID", payload.OperationId);
                var response = await httpClient.SendAsync(requestMessage, token);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync(token);
                    var providerResponse = JsonSerializer.Deserialize<ProviderPaymentResponse>(responseContent);
                    if(providerResponse is null)
                    {
                        await outboxRepository.IncrementRetryAsync(payload.OperationId, token);
                        return;
                    }
                    if(providerResponse.Status == "ACCEPTED")
                    {
                        await operationsRepository.UpdateProviderAsync(payload.OperationId, 
                            providerResponse.ProviderPaymentId, token);
                        Events? lastEv = await eventsRepository.GetLastOperAsync(payload.OperationId, token);
                        if (lastEv is null)
                            return;
                        lastEv.Update("PROVIDER_RESPONSE", "PROCESSING", "Payment accepted by provider");
                        await eventsRepository.UpdateAsync(lastEv, token);
                        await outboxRepository.UpdateStatusAsync(message.Id, "COMPLETED", token);
                    }
                    else
                    {
                        await outboxRepository.IncrementRetryAsync(payload.OperationId, token);
                    }
                }
                else
                {
                    await outboxRepository.IncrementRetryAsync(payload.OperationId, token);
                    return;
                }
                await context.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
            }
            catch
            {
                await transaction.RollbackAsync(token);
            }
        }
    }
}
