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


// Hosted service to run KestrelMetricServer
public class MetricsServerHostedService : IHostedService
{
    private readonly ILogger<MetricsServerHostedService> _logger;
    private readonly KestrelMetricServer _metricServer;

    public MetricsServerHostedService(ILogger<MetricsServerHostedService> logger)
    {
        _logger = logger;
        _metricServer = new KestrelMetricServer(hostname: "0.0.0.0", port: 9090);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting KestrelMetricServer on 0.0.0.0:9090");
        _metricServer.Start();
        _logger.LogInformation("KestrelMetricServer started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping KestrelMetricServer");
        _metricServer.Stop();
        return Task.CompletedTask;
    }
}