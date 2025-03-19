using Ifolor.ConsumerService.Core.Enums;
using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using IfolorConsumerService.Application.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ifolor.ConsumerService.Tests
{
    public class EventProcessorTests
    {
        [Fact]
        public async Task HandleEvent_ProcessesEventAndSavesToRepository()
        {
            // Arrange
            var mockRepository = new Mock<IEventRepository>();
            var mockSensorService = new Mock<ISensorService>();

            var eventProcessor = new EventProcessor(mockRepository.Object, mockSensorService.Object);
            var guid = Guid.NewGuid();

            var eventData = new SensorData
            {
                EventId = guid,
                Timestamp = DateTime.UtcNow,
                MeasurementType = MeasurementType.Temperature,
                SensorId = "sensor-456",
                MeasurementValue = 25.5
            };

            var processedEventData = new SensorEventData
            {
                Data = new SensorData
                {
                    EventId = guid,
                    Timestamp = DateTime.UtcNow,
                    MeasurementType = MeasurementType.Temperature,
                    SensorId = "sensor-456",
                    MeasurementValue = 25.5
                },
                ProccessedTime = DateTime.UtcNow,
                Status = SensorEventStatus.Success,
            };

            // Mock the ProcessSensorEvent method to return the processed event data
            mockSensorService.Setup(s => s.ProcessSensorEvent(eventData))
                .Returns(processedEventData);

            // Act
            await eventProcessor.HandleEvent(eventData);

            // Assert
            // Verify that ProcessSensorEvent was called with the correct event data
            mockSensorService.Verify(s => s.ProcessSensorEvent(eventData), Times.Once);

            // Verify that SaveEventAsync was called with the processed event data
            mockRepository.Verify(r => r.SaveEventAsync(processedEventData), Times.Once);
        }

        [Fact]
        public async Task HandleEvent_ThrowsException_WhenSensorServiceFails()
        {
            // Arrange
            var mockRepository = new Mock<IEventRepository>();
            var mockSensorService = new Mock<ISensorService>();

            var eventProcessor = new EventProcessor(mockRepository.Object, mockSensorService.Object);
            var guid = Guid.NewGuid();

            var eventData = new SensorData
            {
                EventId = guid,
                Timestamp = DateTime.UtcNow,
                MeasurementType = MeasurementType.Temperature,
                SensorId = "sensor-456",
                MeasurementValue = 25.5
            };

            // Simulate an exception when ProcessSensorEvent is called
            mockSensorService.Setup(s => s.ProcessSensorEvent(eventData))
                .Throws(new InvalidOperationException("Sensor processing failed."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                eventProcessor.HandleEvent(eventData));

            // Verify that SaveEventAsync was never called
            mockRepository.Verify(r => r.SaveEventAsync(It.IsAny<SensorEventData>()), Times.Never);
        }

        [Fact]
        public async Task HandleEvent_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            var mockRepository = new Mock<IEventRepository>();
            var mockSensorService = new Mock<ISensorService>();

            var eventProcessor = new EventProcessor(mockRepository.Object, mockSensorService.Object);
            var guid = Guid.NewGuid();

            var eventData = new SensorData
            {
                EventId = guid,
                Timestamp = DateTime.UtcNow,
                MeasurementType = MeasurementType.Temperature,
                SensorId = "sensor-456",
                MeasurementValue = 25.5
            };

            var processedEventData = new SensorEventData
            {
                Data = new SensorData
                {
                    EventId = guid,
                    Timestamp = DateTime.UtcNow,
                    MeasurementType = MeasurementType.Temperature,
                    SensorId = "sensor-456",
                    MeasurementValue = 25.5
                },
                ProccessedTime = DateTime.UtcNow,
                Status = SensorEventStatus.Success,
            };

            // Mock the ProcessSensorEvent method to return the processed event data
            mockSensorService.Setup(s => s.ProcessSensorEvent(eventData))
                .Returns(processedEventData);

            // Simulate an exception when SaveEventAsync is called
            mockRepository.Setup(r => r.SaveEventAsync(processedEventData))
                .ThrowsAsync(new DbUpdateException("Database update failed."));

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(() =>
                eventProcessor.HandleEvent(eventData));

            // Verify that ProcessSensorEvent was called
            mockSensorService.Verify(s => s.ProcessSensorEvent(eventData), Times.Once);

            // Verify that SaveEventAsync was called
            mockRepository.Verify(r => r.SaveEventAsync(processedEventData), Times.Once);
        }
    }
}
