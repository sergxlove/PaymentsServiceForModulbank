using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite.Abstractions;

namespace PaymentsServiceForModulebank.Sqlite.Repositories
{
    public class OperationsRepository : IOperationsRepository
    {
        private readonly PaymengsServiceDbContext _context;
        public OperationsRepository(PaymengsServiceDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateAsync(Operations op, CancellationToken token)
        {
            await _context.AddAsync(op, token);
            await _context.SaveChangesAsync(token);
            return op.OperationId;
        }

        public async Task<bool> CheckAsync(string operationId, CancellationToken token)
        {
            Operations? op = await _context.OperationsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
            if (op is null)
                return false;
            return true;
        }

        public async Task<Operations?> GetAsync(string operationId, CancellationToken token)
        {
            return await _context.OperationsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
        }

        public async Task<OperationStatus?> GetStatusAsync(string operationId, CancellationToken token)
        {
            Operations? op = await _context.OperationsTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
            if (op is null)
                return null;
            return op.Status;
        }


        public async Task<int> UpdateStatusAsync(string operationId, OperationStatus status,
            CancellationToken token)
        {
            return await _context.OperationsTable
                .Where(a => a.OperationId == operationId)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.Status, status), token);
        }

        public async Task<int> UpdateProviderAsync(string operationId, string providerid,  
            CancellationToken token)
        {
            return await _context.OperationsTable
                .Where(a => a.OperationId == operationId)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.ProviderPaymentId, providerid), token);
        }
    }
}
