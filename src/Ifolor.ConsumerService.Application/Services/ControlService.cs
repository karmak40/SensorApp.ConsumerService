using Ifolor.ConsumerService.Infrastructure.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IfolorConsumerService.Application.Services
{
    public class ControlService : IHostedService
    {
        private readonly IMessageConsumer _messageConsumer;
        private readonly ILogger<ControlService> _logger;

        public ControlService(IMessageConsumer messageConsumer, ILogger<ControlService> logger)
        {
            _messageConsumer = messageConsumer;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Control Service is starting.");

            // Start consuming messages
            _messageConsumer.StartConsuming();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Control Service is stopping.");

            // Stop consuming messages
            await _messageConsumer.StopConsumingAsync();
        }
    }
}
