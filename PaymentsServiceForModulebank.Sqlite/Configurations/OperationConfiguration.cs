using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsServiceForModulebank.Core.Models;

namespace PaymentsServiceForModulebank.Sqlite.Configurations
{
    public class OperationConfiguration : IEntityTypeConfiguration<Operations>
    {
        public void Configure(EntityTypeBuilder<Operations> builder)
        {
            
        }
    }
}
