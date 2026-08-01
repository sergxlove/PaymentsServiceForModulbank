using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Abstractions
{
    public interface IOperationsRepository
    {
        Task<bool> CheckAsync(string operationId, CancellationToken token);
        Task<string> CreateAsync(Operations op, CancellationToken token);
        Task<Operations?> GetAsync(string operationId, CancellationToken token);
        Task<int> UpdateStatusAsync(string operationId, OperationStatus status, CancellationToken token);
    }
}