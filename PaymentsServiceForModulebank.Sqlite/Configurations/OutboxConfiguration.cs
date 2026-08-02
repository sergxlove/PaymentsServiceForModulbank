using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Configurations
{
    public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessages>
    {
        public void Configure(EntityTypeBuilder<OutboxMessages> builder)
        {
            
        }
    }
}
