using Ifolor.ConsumerService.Core.Models;
using Ifolor.ConsumerService.Core.Services;
using Ifolor.ConsumerService.Infrastructure.Persistance;

namespace IfolorConsumerService.Application.Services
{
    /// <summary>
    /// Controlls event handling
    /// </summary>
    public class EventProcessor : IEventProcessor
    {
        private readonly IEventRepository _repository;
        private readonly ISensorService _sensorService;

        public EventProcessor(IEventRepository repository, ISensorService sensorServic)
        {
            _repository = repository;
            _sensorService = sensorServic;
        }

        public async Task HandleEvent(SensorData eventData)
        {
            //proccess event
            var sensorEventData = _sensorService.ProcessSensorEvent(eventData);

            // saving to SQLite 
            await _repository.SaveEventAsync(sensorEventData);
        }

    }
}
