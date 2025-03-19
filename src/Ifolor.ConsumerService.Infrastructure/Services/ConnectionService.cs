using Ifolor.ConsumerService.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ifolor.ConsumerService.Infrastructure.Services
{
    public class ConnectionService : IConnectionService
    {
        private ConsumerPolicyConfig _consumerPolicyConfig;
        private readonly ILogger<ConnectionService> _logger;

        public ConnectionService(ILogger<ConnectionService> logger, IOptions<ConsumerPolicyConfig> consumerPolicyConfig) 
        {
            _consumerPolicyConfig = consumerPolicyConfig.Value;
            _logger = logger;
        }

        /// <summary>
        /// Try to create connection, if failed, retry after inteval
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IConnection> CreateConnectionWithRetryAsync(IConnectionFactory factory, CancellationToken cancellationToken)
        {
            var maxRetries = _consumerPolicyConfig.MaxConnectionRetry;
            var maxConnectionDelay = _consumerPolicyConfig.DelayBetweenConnectionRetryInSeconds;
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
                        //get outside loop after max retry
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(maxConnectionDelay), cancellationToken);
                }
            }
            throw new InvalidOperationException("Failed to connect to RabbitMQ after multiple retries.");
        }
    }
}
