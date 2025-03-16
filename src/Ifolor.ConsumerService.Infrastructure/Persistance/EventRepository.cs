using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Entities;
using System.Text.Json;

namespace Ifolor.ConsumerService.Infrastructure.Persistance
{
    public class EventRepository : IEventRepository
    {
        private readonly ConsumerDbContext _context;

        public EventRepository(ConsumerDbContext context)
        {
            _context = context;
        }

        public async Task SaveEventAsync(SensorEventData sensorEventData)
        {
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

            _context.SensorEvents.Add(entity);
            await _context.SaveChangesAsync();
        }
    }
}
