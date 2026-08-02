using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite.Abstractions;

namespace PaymentsServiceForModulebank.Sqlite.Repositories
{
    public class EventsRepository : IEventsRepository
    {
        private readonly PaymengsServiceDbContext _context;
        public EventsRepository(PaymengsServiceDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Events ev, CancellationToken token)
        {
            await _context.EventsTable.AddAsync(ev, token);
            await _context.SaveChangesAsync(token);
            return ev.Id;
        }

        public async Task<List<Events>> GetByIdOperAsync(string operationId, CancellationToken token)
        {
            return await _context.EventsTable
                .AsNoTracking()
                .Where(a => a.OperationId == operationId)
                .ToListAsync(token);
        }

        public async Task<Events?> GetLastOperAsync(string operationId, CancellationToken token)
        {
            Events? result = null;
            List<Events> all = await _context.EventsTable
                .AsNoTracking()
                .Where(a => a.OperationId == operationId)
                .ToListAsync(token);
            foreach (Events e in all)
            {
                if (result is null)
                {
                    result = e;
                    continue;
                }
                if (result.EventId < e.EventId)
                    result = e;
            }
            return result;
        }
    }
}
