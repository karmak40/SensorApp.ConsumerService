using Ifolor.ConsumerService.Core.Models;

namespace Ifolor.ConsumerService.Core.Services
{
    public interface IEventProcessor
    {
        Task HandleEvent(SensorData eventData);
    }
}
