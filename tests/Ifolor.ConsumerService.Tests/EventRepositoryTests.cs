using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Ifolor.ConsumerService.Tests
{
    public class EventRepositoryTests
    {
        private readonly Mock<IDbContextFactory<ConsumerDbContext>> _mockContextFactory;

        public EventRepositoryTests()
        {
            _mockContextFactory = new Mock<IDbContextFactory<ConsumerDbContext>>();

        }

        [Fact]
        public async Task SaveEventAsync_SavesEventToDatabase()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext("testdb");
            _mockContextFactory
                .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
                .Returns(Task.FromResult(dbContext));

            var eventRepository = new EventRepository(_mockContextFactory.Object);

            var guid = Guid.NewGuid();

            var sensorEventData = new SensorEventData
            {
                Data = new SensorData
                {
                    EventId =  guid,
                    Timestamp = DateTime.UtcNow,
                    MeasurementType = Core.Enums.MeasurementType.Temperature,
                    SensorId = "sensor-456",
                    MeasurementValue = 25.5
                },
                ProccessedTime = DateTime.UtcNow,
                Status = SensorEventStatus.Success
            };

            // Act
            await eventRepository.SaveEventAsync(sensorEventData);

            // Assert
            using (dbContext = CreateInMemoryDbContext("testdb"))
            {
                var savedEntity = await dbContext.SensorEvents.FirstOrDefaultAsync();
                Assert.NotNull(savedEntity);
                Assert.Equal(savedEntity.EventId, sensorEventData.Data.EventId);
                Assert.Equal(savedEntity.SensorId, sensorEventData.Data.SensorId);
            }
        }

        private ConsumerDbContext CreateInMemoryDbContext(string testDb)
        {
            var options = new DbContextOptionsBuilder<ConsumerDbContext>()
                .UseInMemoryDatabase(databaseName: testDb)
                .Options;

            return new ConsumerDbContext(options);
        }
    }
}