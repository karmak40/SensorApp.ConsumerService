using Ifolor.ConsumerService.Application.Metric;
using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Ifolor.ConsumerService.Infrastructure.Messaging
{
    /// <summary>
    /// Service responsible for gecieving messages from RabbitMQ Message broker
    /// </summary>
    public class RabbitMQConsumer : IMessageConsumer
    {
        private const int MaxDegreeOfParallelism = 10;
        private readonly SemaphoreSlim _semaphore;

        private readonly IEventProcessor _eventProcessor;
        private readonly IConnectionService _connectionService;

        private readonly ILogger<RabbitMQConsumer> _logger;
        private RabbitMQConfig _rabbitMQConfig;

        private readonly BlockingCollection<SensorData> _messageQueue = new BlockingCollection<SensorData>();
        private Task _consumingTask;
        private Task _processingTask;

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        public RabbitMQConsumer(
            IEventProcessor eventProcessor,
            IConnectionService connectionService,
            ILogger<RabbitMQConsumer> logger,
            IOptions<RabbitMQConfig> rabbitMQConfig
            )
        {
            _eventProcessor = eventProcessor;
            _rabbitMQConfig = rabbitMQConfig.Value;
            _connectionService = connectionService;
            _logger = logger;

            _semaphore = new SemaphoreSlim(MaxDegreeOfParallelism);
        }

        public void StartConsuming()
        {
            _logger.LogInformation("Starting RabbitMQ Consumer.");

            // Start the message processing task
            _processingTask = Task.Run(() => ProcessMessagesAsync(_stoppingCts.Token), _stoppingCts.Token);

            // Start the message consuming task
            _consumingTask = Task.Run(() => ConsumeMessagesAsync(_stoppingCts.Token), _stoppingCts.Token);
        }

        public async Task StopConsumingAsync()
        {
            _logger.LogInformation("Stopping RabbitMQ Consumer.");

            // Signal cancellation
            _stoppingCts.Cancel();

            // Wait for both tasks to complete
            await Task.WhenAll(_consumingTask, _processingTask);

            _logger.LogInformation("RabbitMQ Consumer has stopped.");
        }

        private async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {

                try
                {
                    IConnectionFactory factory = new ConnectionFactory
                    {
                        HostName = _rabbitMQConfig.HostName,
                        UserName = _rabbitMQConfig.Username,
                        Password = _rabbitMQConfig.Password,
                    };

                    using var connection = await _connectionService.CreateConnectionWithRetryAsync(factory, cancellationToken);
                    using var channel = await connection.CreateChannelAsync();

                    await channel.QueueDeclareAsync(
                        queue: _rabbitMQConfig.QueueName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation(" [*] Waiting for messages.");

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        try
                        {
                            ConsumerMetrics.MessagesConsumed.Inc();
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var eventData = JsonSerializer.Deserialize<SensorData>(message);

                            // Add the message to the blocking collection
                            _messageQueue.Add(eventData, cancellationToken);
                            ConsumerMetrics.MessagesInQueue.Inc();

                            // Manually acknowledge the message
                            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                        }
                        catch (JsonException ex)
                        {
                            ConsumerMetrics.MessagesFailed.Inc();
                            // implement dead letter queue ??
                        }

                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing message");
                            ConsumerMetrics.MessagesFailed.Inc();

                            // Reject the message and requeue it
                            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                        }
                    };

                    await channel.BasicConsumeAsync(
                        queue: _rabbitMQConfig.QueueName,
                        autoAck: false, // Manual acknowledgment
                        consumer: consumer,
                        cancellationToken: cancellationToken);

                    // Keep the task running until cancellation is requested
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Consumer operation was canceled.");
                    break; // Exit the loop if cancellation is requested
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ConsumeMessagesAsync");

                    // Wait before retrying to avoid overwhelming the system
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }
        }

        private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable(cancellationToken))
            {
                await _semaphore.WaitAsync(cancellationToken);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _eventProcessor.HandleEvent(message);
                        ConsumerMetrics.MessagesProcessed.Inc();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling event");
                        ConsumerMetrics.MessagesFailed.Inc();
                    }
                    finally
                    {
                        _semaphore.Release();
                        ConsumerMetrics.MessagesInQueue.Dec();
                    }
                }, cancellationToken);
            }
        }
    }
}
