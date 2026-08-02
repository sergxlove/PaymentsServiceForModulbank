using PaymentsServiceForModulbank.API.Requests;
using PaymentsServiceForModulbank.API.Responses;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite;
using PaymentsServiceForModulebank.Sqlite.Abstractions;
using System.Text.Json;

namespace PaymentsServiceForModulbank.API.Endpoints
{
    public static class OperationEndpoints
    {
        public static IEndpointRouteBuilder MapOperationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health", () =>
            {
                return Results.Ok();
            });

            app.MapPost("/operations", async (HttpContext context, 
                OperationCreateRequest request,
                IOperationsRepository operationsRepository,
                CancellationToken token) =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(request.OperationId))
                    {
                        return Results.BadRequest("operationId is required");
                    }

                    if (request.Amount <= 0)
                    {
                        return Results.BadRequest("amount must be positive" );
                    }

                    if (request.Currency != "RUB")
                    {
                        return Results.BadRequest("only RUB currency is supported");
                    }

                    Operations newOp = new ()
                    {
                        OperationId = request.OperationId,
                        Amount = request.Amount,
                        Currency = request.Currency,
                        Description = request.Description,
                        Status = OperationStatus.CREATED,
                        ProviderPaymentId = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        RetryCount = 0
                    };
                    bool resultCheck = await operationsRepository.CheckAsync(newOp.OperationId, token);
                    if (resultCheck)
                        return Results.Conflict();
                    string resultCreate = await operationsRepository.CreateAsync(newOp, token);
                    if (resultCreate != newOp.OperationId)
                        throw new Exception();
                    OperationCreateResponse result = new()
                    {
                        OperationId = newOp.OperationId,
                        Amount = newOp.Amount,
                        Currency = newOp.Currency,
                        Description = newOp.Description,
                        Status = newOp.Status,
                        ProviderPaymentId = newOp.ProviderPaymentId
                    };
                    return Results.Created($"/operations/{newOp.OperationId}", result);
                }
                catch
                {
                    return Results.InternalServerError();
                }
            });

            app.MapPost("/operations/{id}/submit", async (string id,
                PaymengsServiceDbContext db,
                IOperationsRepository operationsRepository,
                IOutboxRepository outboxRepository,
                CancellationToken token) =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                try
                {
                    Operations? op = await operationsRepository.GetAsync(id, token);
                    if (op is null)
                        return Results.NotFound();
                    if (op.Status != OperationStatus.CREATED)
                        return Results.Ok();
                    if(!await outboxRepository.CheckAsync(op.OperationId, token))
                    {
                        OutboxMessages om = new()
                        {
                            Id = Guid.NewGuid(),
                            OperationId = op.OperationId,
                            Payload = JsonSerializer.Serialize(op),
                            Status = "PENDING",
                            CreatedAt = DateTime.UtcNow,
                            RetryCount = 0
                        };
                        Guid resultCreateOutbox = await outboxRepository.CreateAsync(om, token);
                        if (resultCreateOutbox != om.Id)
                            throw new Exception();
                    }

                    int resultUpdate = await operationsRepository.UpdateStatusAsync(op.OperationId, 
                        OperationStatus.PROCESSING, token);
                    if(resultUpdate == 0)
                        throw new Exception();

                    await db.SaveChangesAsync(token);
                    await transaction.CommitAsync(token);
                    return Results.Accepted();
                }
                catch
                {
                    await transaction.RollbackAsync(token);
                    return Results.InternalServerError();
                }
            });

            app.MapGet("/operations/{id}", async (string id,
                IOperationsRepository operationsRepository,
                CancellationToken token) =>
            {
                try
                {
                    OperationStatus? status = await operationsRepository.GetStatusAsync(id, token);
                    if (status is null)
                        return Results.NotFound();
                    return Results.Ok(status);
                }
                catch
                {
                    return Results.InternalServerError();
                }
            });

            app.MapGet("/operations/{id}/events", async (string id,
                IEventsRepository eventsRepository,
                CancellationToken token) =>
            {
                try
                {
                    List<Events> result = await eventsRepository.GetByIdOperAsync(id, token);
                    return Results.Ok(result);
                }
                catch
                {
                    return Results.InternalServerError();
                }
            });

            return app;
        }
    }
}
