using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulebank.Core.Models;
using PaymentsServiceForModulebank.Sqlite.Configurations;

namespace PaymentsServiceForModulebank.Sqlite
{
    public class PaymengsServiceDbContext : DbContext
    {
        public PaymengsServiceDbContext(DbContextOptions<PaymengsServiceDbContext> options)
            : base(options) { }
        
        public DbSet<Operations> OperationsTable { get; set; }
        public DbSet<OutboxMessages> OutboxTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OperationConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxConfigurations());
            base.OnModelCreating(modelBuilder);
        }
    }
}
