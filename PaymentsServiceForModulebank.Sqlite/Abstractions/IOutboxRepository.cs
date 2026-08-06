using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Abstractions
{
    public interface IOutboxRepository
    {
        Task<Guid> CreateAsync(OutboxMessages om, CancellationToken token);
        Task<OutboxMessages?> GetAsync(string operationId, CancellationToken token);
        Task<bool> CheckAsync(string operationId, CancellationToken token);
        Task<List<OutboxMessages>> GetByStatusAsync(string status, int take, CancellationToken token);
        Task<List<OutboxMessages>> GetByStatusAsync(List<string> status, int take, CancellationToken token);
        Task<int> UpdateStatusAsync(Guid id, string status, CancellationToken token);
        Task<int> IncrementRetryAsync(string operationId, CancellationToken token);
        Task<int?> GetCountAsync(string operationId, CancellationToken token);
    }
}