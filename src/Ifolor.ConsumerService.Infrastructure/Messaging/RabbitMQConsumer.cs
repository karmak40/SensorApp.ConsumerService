using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Ifolor.ConsumerService.Infrastructure.Messaging
{
    public class RabbitMQConsumer : IHostedService
    {
        private readonly IEventProcessor _eventProcessor;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public RabbitMQConsumer(IEventProcessor eventProcessor, ILogger<RabbitMQConsumer> logger) 
        {
            _eventProcessor = eventProcessor;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service is starting.");

            _executingTask = ExecuteAsync(_stoppingCts.Token);

            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMQ Consumer Service is stopping.");

            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // expected stop
                _stoppingCts.Cancel();
            }
            finally
            {
                // expecting ending task
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "sendordata", durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var eventData = JsonSerializer.Deserialize<SensorData>(message);

                    await _eventProcessor.HandleEvent(eventData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                }
            };

            await channel.BasicConsumeAsync("sendordata", autoAck: true, consumer: consumer);

            _logger.LogInformation("Consumer started. Waiting for messages...");

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
    }
}
