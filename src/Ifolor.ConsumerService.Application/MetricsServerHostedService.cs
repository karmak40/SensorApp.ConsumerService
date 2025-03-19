using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;


/// <summary>
/// Hosted service to run KestrelMetricServer
/// workaround to make metrics available for prometheus 
/// </summary>

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