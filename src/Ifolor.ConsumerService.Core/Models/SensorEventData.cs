using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifolor.ConsumerService.Core.Models
{
    public class SensorEventData
    {
        public DateTime ProccessedTime { get; set; }
        public SensorEventStatus Status { get; set; }
        public required SensorData Data { get; set; }
    }

    public enum SensorEventStatus
    {
        Success,
        Failure
    }
}
