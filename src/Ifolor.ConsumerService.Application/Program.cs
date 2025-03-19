using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Messaging;
using Ifolor.ConsumerService.Infrastructure.Persistance;
using Ifolor.ConsumerService.Infrastructure.Services;
using IfolorConsumerService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SQLiteConfig>(context.Configuration.GetSection("SQLite"));
                    services.Configure<RabbitMQConfig>(context.Configuration.GetSection("RabbitMQ"));

                    services.Configure<ConsumerPolicyConfig>(context.Configuration.GetSection("ConsumerPolicy"));
                    // Register metrics
                    services.AddMetrics();

                    // SQLite
                    services.AddDbContextFactory<ConsumerDbContext>((serviceProvider, options) =>
                    {
                        var sqliteConfig = serviceProvider.GetRequiredService<IOptions<SQLiteConfig>>();
                        var connectionString = sqliteConfig.Value.ConnectionString;

                        if (string.IsNullOrEmpty(connectionString))
                        {
                            throw new InvalidOperationException("SQLite connection string is null or empty.");
                        }
                        options.UseSqlite(connectionString);
                    });
                    
                    services.AddScoped<IEventRepository, EventRepository>();
                    services.AddScoped<IEventProcessor, EventProcessor>();
                    services.AddScoped<ISensorService, SensorService>();
                    services.AddScoped<IMessageConsumer, RabbitMQConsumer>();
                    services.AddScoped<IConnectionService, ConnectionService>();

                    services.AddHostedService<ControlService>();

                    // Register KestrelMetricServer as a hosted service
                    services.AddHostedService<MetricsServerHostedService>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsumerDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();
