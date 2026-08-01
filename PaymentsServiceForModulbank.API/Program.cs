using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulbank.API.Endpoints;
using PaymentsServiceForModulebank.Sqlite;
using PaymentsServiceForModulebank.Sqlite.Abstractions;
using PaymentsServiceForModulebank.Sqlite.Repositories;

namespace PaymentsServiceForModulbank.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<PaymengsServiceDbContext>(options =>
                options.UseSqlite("Data Source=/data/payments.db"));
            builder.Services.AddScoped<IOperationsRepository, OperationsRepository>();
            builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
            var app = builder.Build();

            app.MapOperationEndpoints();

            app.Run();
        }
    }
}
