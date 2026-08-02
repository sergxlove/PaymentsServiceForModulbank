using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Abstractions
{
    public interface IEventsRepository
    {
        Task<Guid> CreateAsync(Events ev, CancellationToken token);
        Task<List<Events>> GetByIdOperAsync(string operationId, CancellationToken token);
        Task<Events?> GetLastOperAsync(string operationId, CancellationToken token);
        Task<int> UpdateAsync(Events ev, CancellationToken token);
    }
}