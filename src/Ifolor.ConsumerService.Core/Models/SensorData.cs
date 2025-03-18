using Ifolor.ConsumerService.Core.Enums;

namespace Ifolor.ConsumerService.Core.Models
{
    public class SensorData
    {
        public required string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double MeasurementValue { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public Guid EventId { get; set; }
        public EventStatus EventStatus { get; set; }
    }
}
