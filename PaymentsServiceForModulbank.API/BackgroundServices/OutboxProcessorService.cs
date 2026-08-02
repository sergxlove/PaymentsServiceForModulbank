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
        private readonly int _maxRetryCount = 3;
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
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>()
                .CreateClient("ProviderClient");
            try
            {
                List<OutboxMessages> mes = await outboxService.GetByStatusAsync("PENDING", 10, 
                    cancellationToken);
                if (!mes.Any())
                    return;
                foreach(OutboxMessages m in mes)
                {
                    await ProcessSingleMessage(m, httpClient, outboxService, cancellationToken);
                }
            }
            catch
            {

            }
        }

        private async Task ProcessSingleMessage(OutboxMessages message, HttpClient httpClient, 
            IOutboxRepository repository, CancellationToken token)
        {
            try
            {
                int resultUpdate = await repository.UpdateStatusAsync(message.Id, "PROCESSING", token);
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

                }
                else
                {

                }
            }
            catch
            {

            }
        }
    }
}
