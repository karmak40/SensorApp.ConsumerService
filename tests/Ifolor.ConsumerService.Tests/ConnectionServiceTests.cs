using Ifolor.ConsumerService.Infrastructure.Messaging;
using Ifolor.ConsumerService.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifolor.ConsumerService.Tests
{
    public class ConnectionServiceTests
    {
        private readonly Mock<ILogger<ConnectionService>> _mockLogger;
        private readonly ConsumerPolicyConfig consumerPolicyConfig;

        IOptions<ConsumerPolicyConfig> _consumerPolicyConfig;

        public ConnectionServiceTests()
        {
            _mockLogger = new Mock<ILogger<ConnectionService>>();
        }


        [Fact]
        public async Task CreateConnectionWithRetryAsync_SuccessOnFirstAttempt_ReturnsConnection()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ConnectionService>>();
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockConnection.Object);

            var mockOptions = new Mock<IOptions<ConsumerPolicyConfig>>();
            mockOptions.Setup(o => o.Value) 
                .Returns(GetPolicy()); 

            var connectionService = new ConnectionService(mockLogger.Object, mockOptions.Object);

            // Act
            var result = await connectionService.CreateConnectionWithRetryAsync(mockFactory.Object, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockConnection.Object, result);
            mockFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateConnectionWithRetryAsync_RetriesOnFailure_SucceedsAfterRetries()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ConnectionService>>();
            var mockConnection = new Mock<IConnection>();
            var mockFactory = new Mock<IConnectionFactory>();

            var callCount = 0;
            mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    if (callCount <= 2) // Fail the first two attempts
                    {
                        throw new Exception("Connection failed.");
                    }
                    return mockConnection.Object; // Succeed on the third attempt
                });

            var mockOptions = new Mock<IOptions<ConsumerPolicyConfig>>();
            mockOptions.Setup(o => o.Value)
                .Returns(GetPolicy()); 

            var connectionService = new ConnectionService(mockLogger.Object, mockOptions.Object);

            // Act
            var result = await connectionService.CreateConnectionWithRetryAsync(mockFactory.Object, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockConnection.Object, result);
            mockFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
        }

        [Fact]
        public async Task CreateConnectionWithRetryAsync_ThrowsException_AfterMaxRetries()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ConnectionService>>();
            var mockFactory = new Mock<IConnectionFactory>();

            mockFactory.Setup(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Connection failed."));

            var mockOptions = new Mock<IOptions<ConsumerPolicyConfig>>();
            mockOptions.Setup(o => o.Value) 
                .Returns(GetPolicy()); 

            var connectionService = new ConnectionService(mockLogger.Object, mockOptions.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                connectionService.CreateConnectionWithRetryAsync(mockFactory.Object, CancellationToken.None));

            mockFactory.Verify(f => f.CreateConnectionAsync(It.IsAny<CancellationToken>()), Times.Exactly(5));
   
        }

        private ConsumerPolicyConfig GetPolicy()
        {
            return new ConsumerPolicyConfig
            {
                DelayBetweenConnectionRetryInSeconds = 1,
                MaxConnectionRetry = 5,
                ResendDelayInSeconds = 1,
            };
        }
    }
}
