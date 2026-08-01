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

        public async Task<bool> CheckAsync(string operationId, CancellationToken token)
        {
            OutboxMessages? result = await _context.OutboxTable
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.OperationId == operationId, token);
            if (result is null)
                return false;
            return true;
        }
    }
}
