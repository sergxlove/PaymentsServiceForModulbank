using Microsoft.EntityFrameworkCore;
using PaymentsServiceForModulbank.API.BackgroundServices;
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
            builder.Services.AddScoped<IEventsRepository, EventsRepository>();
            builder.Services.AddScoped<IOperationsRepository, OperationsRepository>();
            builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
            //builder.Services.AddHostedService<OutboxProcessorService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapOperationEndpoints();
            app.Run();
        }
    }
}
