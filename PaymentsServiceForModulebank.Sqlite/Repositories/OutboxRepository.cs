using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite.Abstractions;

namespace PaymentsServiceForModulebank.Sqlite.Repositories
{
    public class OutboxRepository : IOutboxRepository
    {
        private readonly PaymengsServiceDbContext _context;
        public OutboxRepository(PaymengsServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(OutboxMessages om, CancellationToken token)
        {
            await _context.OutboxTable.AddAsync(om, token);
            await _context.SaveChangesAsync(token);
            return om.Id;
        }

        public async Task<OutboxMessages?> GetAsync(string operationId, CancellationToken token)
        {
            return await _context.OutboxTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
        }

        public async Task<List<OutboxMessages>> GetByStatusAsync(string status, int take,
            CancellationToken token)
        {
            return await _context.OutboxTable
                .Where(a => a.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .ToListAsync(token);
        }

        public async Task<int?> GetCountAsync(string operationId, CancellationToken token)
        {
            OutboxMessages? result = await _context.OutboxTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
            if (result is null)
                return null;
            return result.RetryCount;
        }

        public async Task<List<OutboxMessages>> GetByStatusAsync(List<string> status, int take,
            CancellationToken token)
        {
            return await _context.OutboxTable
                .Where(a => status.Contains(a.Status))
                .OrderByDescending(o => o.CreatedAt)
                .Take(take)
                .ToListAsync(token);
        }

        public async Task<int> UpdateStatusAsync(Guid id, string status, CancellationToken token)
        {
            return await _context.OutboxTable
                .Where(a => a.Id == id)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.Status, status), token);
        }

        public async Task<bool> CheckAsync(string operationId, CancellationToken token)
        {
            OutboxMessages? result = await _context.OutboxTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
            if (result is null)
                return false;
            return true;
        }

        public async Task<int> IncrementRetryAsync(string operationId, CancellationToken token)
        {
            return await _context.OutboxTable
                .Where(a => a.OperationId == operationId)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.RetryCount, a => a.RetryCount + 1), token);
        }
    }
}
