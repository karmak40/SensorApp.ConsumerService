using Ifolor.ConsumerService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ifolor.ConsumerService.Core.Services
{
    public interface ISensorService
    {
        SensorEventData ProcessSensorEvent(SensorData eventData);
    }
}
