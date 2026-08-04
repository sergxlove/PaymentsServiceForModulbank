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
                PaymengsServiceDbContext db,
                IOperationsRepository operationsRepository,
                IEventsRepository eventsRepository,
                CancellationToken token) =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(token);
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
                    Events newEvent = Events.Init(newOp.OperationId);
                    await eventsRepository.CreateAsync(newEvent, token);
                    OperationCreateResponse result = new()
                    {
                        OperationId = newOp.OperationId,
                        Amount = newOp.Amount,
                        Currency = newOp.Currency,
                        Description = newOp.Description,
                        Status = newOp.Status,
                        ProviderPaymentId = newOp.ProviderPaymentId
                    };
                    await db.SaveChangesAsync(token);
                    await transaction.CommitAsync(token);
                    return Results.Created($"/operations/{newOp.OperationId}", result);
                }
                catch
                {
                    await transaction.RollbackAsync(token);
                    return Results.InternalServerError();
                }
            });

            app.MapPost("/operations/{id}/submit", async (string id,
                PaymengsServiceDbContext db,
                IOperationsRepository operationsRepository,
                IOutboxRepository outboxRepository,
                IEventsRepository eventsRepository,
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
                        Events? ev = await eventsRepository.GetLastOperAsync(op.OperationId, token);
                        if(ev is null)
                            throw new Exception();
                        ev.Update("SUBMIT", "SUBMIT", "Operation submitted for processing");
                        await eventsRepository.CreateAsync(ev, token);
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

            app.MapPost("/receipts", async (CallBackRequest request,
                PaymengsServiceDbContext db,
                IOperationsRepository operationsRepository,
                IEventsRepository eventsRepository,
                CancellationToken token) =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync(token);
                try
                {
                    if (request is null)
                        return Results.BadRequest();
                    Operations? oper = await operationsRepository.GetAsync(request.OperationId, token);
                    if (oper is null)
                        throw new Exception();
                    if(oper.ProviderPaymentId is null)
                    {
                        int resultUpdateProvider = await operationsRepository.UpdateProviderAsync(
                            oper.OperationId, request.ProviderPaymentId, token);
                        oper.ProviderPaymentId = request.ProviderPaymentId;
                        if( resultUpdateProvider == 0) 
                            throw new Exception();
                    }
                    else
                    {
                        if (oper.ProviderPaymentId != request.ProviderPaymentId)
                            return Results.Conflict();
                    }
                    if(oper.Status == OperationStatus.COMPLETED 
                        || oper.Status == OperationStatus.REJECTED)
                        return Results.NoContent();
                    if (oper.Status == OperationStatus.COMPLETED && request.Result == "REJECTED"
                        || oper.Status == OperationStatus.REJECTED && request.Result == "COMPLETED")
                        return Results.NoContent();
                    OperationStatus finalStatus;
                    switch(request.Result)
                    {
                        case "REJECTED":
                            finalStatus = OperationStatus.REJECTED;
                            break;
                        case "COMPLETED":
                            finalStatus = OperationStatus.COMPLETED;
                            break;
                        default:
                            return Results.BadRequest();
                    }
                    int resultUpdateStatus = await operationsRepository.UpdateStatusAsync(oper.OperationId,
                        finalStatus, token);
                    if (resultUpdateStatus == 0)
                        throw new Exception();
                    Events? ev = await eventsRepository.GetLastOperAsync(oper.OperationId, token);
                    if (ev is null)
                        throw new Exception();
                    ev.Update($"{request.Result}", $"{request.Result}", $"Operation {request.Result}");
                    await eventsRepository.CreateAsync(ev, token);
                    await db.SaveChangesAsync(token);
                    await transaction.CommitAsync(token);
                    return Results.NoContent();
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
                    return Results.Ok(Operations.StatusToString(status.Value));
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
