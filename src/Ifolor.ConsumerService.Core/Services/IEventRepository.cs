using Ifolor.ConsumerService.Core.Models;

namespace Ifolor.ConsumerService.Core.Services
{
    public interface IEventRepository
    {
        Task SaveEventAsync(SensorEventData sensorEventData);
    }
}
