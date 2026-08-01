using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Abstractions
{
    public interface IOutboxRepository
    {
        Task<Guid> CreateAsync(OutboxMessages om, CancellationToken token);
        Task<OutboxMessages?> GetAsync(string operationId, CancellationToken token);
        Task<bool> CheckAsync(string operationId, CancellationToken token);
    }
}