using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Messaging;
using Ifolor.ConsumerService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Concurrent;

namespace Ifolor.ConsumerService.Tests
{
    public class RabbitMQConsumerTests
    {
        [Fact]
        public async Task StartConsuming_StartsProcessingAndConsumingTasks()
        {
            // Arrange
            var mockEventProcessor = new Mock<IEventProcessor>();
            var mockConnectionService = new Mock<IConnectionService>();
            var mockLogger = new Mock<ILogger<RabbitMQConsumer>>();
            var mockRabbitMQConfig = new Mock<IOptions<RabbitMQConfig>>();

            var rabbitMQConfig = new RabbitMQConfig
            {
                HostName = "localhost",
                Username = "guest",
                Password = "guest",
                QueueName = "test-queue"
            };

            mockRabbitMQConfig.Setup(c => c.Value).Returns(rabbitMQConfig);

            var rabbitMQConsumer = new RabbitMQConsumer(
                mockEventProcessor.Object,
                mockConnectionService.Object,
                mockLogger.Object,
                mockRabbitMQConfig.Object
            );

            // Act
            rabbitMQConsumer.StartConsuming();

            // Assert
            // Verify that the processing and consuming tasks are started
            await Task.Delay(1000); // Allow time for tasks to start
            Assert.NotNull(rabbitMQConsumer.GetProcessingTask());
            Assert.NotNull(rabbitMQConsumer.GetConsumingTask());
        }

    }

    // Helper methods to access private members for testing
    public static class RabbitMQConsumerTestExtensions
    {
        public static Task GetProcessingTask(this RabbitMQConsumer consumer)
        {
            return consumer.GetType()
                .GetField("_processingTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(consumer) as Task;
        }

        public static Task GetConsumingTask(this RabbitMQConsumer consumer)
        {
            return consumer.GetType()
                .GetField("_consumingTask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(consumer) as Task;
        }

        public static BlockingCollection<SensorData> GetMessageQueue(this RabbitMQConsumer consumer)
        {
            return consumer.GetType()
                .GetField("_messageQueue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(consumer) as BlockingCollection<SensorData>;
        }
    }
}
