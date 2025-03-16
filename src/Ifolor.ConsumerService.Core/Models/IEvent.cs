namespace Ifolor.ConsumerService.Core.Models
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
        string EventType { get; }
    }
}
