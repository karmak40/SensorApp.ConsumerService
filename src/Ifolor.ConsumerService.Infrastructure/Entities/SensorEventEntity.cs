using Ifolor.ConsumerService.Core.Enums;
using Ifolor.ConsumerService.Core.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ifolor.ConsumerService.Infrastructure.Entities
{
    public class SensorEventEntity
    {
        public int Id { get; set; }
        public Guid EventId { get; set; }
        public required string SensorId { get; set; }
        public DateTime EventOccureTime { get; set; }
        public DateTime EventProcessedTime { get; set; }
        public double MeasurementValue { get; set; }
        public MeasurementType MeasurementType { get; set; }
        public SensorEventStatus Status { get; internal set; }
    }
}
