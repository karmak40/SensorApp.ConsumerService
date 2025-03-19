using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ifolor.ConsumerService.Infrastructure.Persistance
{
    /// <summary>
    /// Repository responsible for saving messages to the DB
    /// </summary>
    public class EventRepository : IEventRepository
    {
        private readonly IDbContextFactory<ConsumerDbContext> _contextFactory;

        public EventRepository(IDbContextFactory<ConsumerDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task SaveEventAsync(SensorEventData sensorEventData)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var entity = new SensorEventEntity
            {
                EventId = sensorEventData.Data.EventId,
                EventOccureTime = sensorEventData.Data.Timestamp,
                MeasurementType = sensorEventData.Data.MeasurementType,
                SensorId = sensorEventData.Data.SensorId,
                MeasurementValue = sensorEventData.Data.MeasurementValue,
                EventProcessedTime = sensorEventData.ProccessedTime,
                Status = sensorEventData.Status,
            };

            context.SensorEvents.Add(entity);
            await context.SaveChangesAsync();
        }
    }
}
