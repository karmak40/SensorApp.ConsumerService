namespace Ifolor.ConsumerService.Core.Models
{
    public class SensorData
    {
        public Guid EventId { get; set; }
        public required string SensorId { get; set; }
        public DateTime Timestamp { get; set; }
        public double MeasurementValue { get; set; }
        public required string MeasurementType { get; set; }
    }
}
