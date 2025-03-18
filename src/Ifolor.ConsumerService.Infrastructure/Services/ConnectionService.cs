using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Ifolor.ConsumerService.Infrastructure.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(ILogger<ConnectionService> logger) 
        {
            _logger = logger;
        }

        public async Task<IConnection> CreateConnectionWithRetryAsync(ConnectionFactory factory, CancellationToken cancellationToken)
        {
            const int maxRetries = 5;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    return await factory.CreateConnectionAsync();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogError(ex, $"Failed to connect to RabbitMQ (Attempt {retryCount}/{maxRetries})");

                    if (retryCount >= maxRetries)
                    {
                        throw; // Re-throw the exception after max retries
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }

            throw new InvalidOperationException("Failed to connect to RabbitMQ after multiple retries.");
        }
    }
}
