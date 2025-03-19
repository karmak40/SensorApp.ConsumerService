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
using Prometheus;

var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SQLiteConfig>(context.Configuration.GetSection("SQLite"));
                    services.Configure<RabbitMQConfig>(context.Configuration.GetSection("RabbitMQ"));

                    services.Configure<ConsumerPolicyConfig>(context.Configuration.GetSection("ConsumerPolicy"));

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

                    // Add Prometheus metrics
                    services.AddSingleton<MetricServer>(sp =>
                    {
                        // Start a lightweight HTTP server to expose /metrics endpoint on port 9090
                        return new MetricServer(hostname: "0.0.0.0", port: 9090);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .Build();

// Start the MetricServer manually since it's not an IHostedService
var metricServer = host.Services.GetRequiredService<MetricServer>();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
try
{
    logger.LogInformation("Starting MetricServer on 0.0.0.0:9090");
    metricServer.Start();
    logger.LogInformation("MetricServer started");
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to start MetricServer");
    throw;
}

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConsumerDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();

// Stop the MetricServer when the host shuts down
metricServer.Stop();