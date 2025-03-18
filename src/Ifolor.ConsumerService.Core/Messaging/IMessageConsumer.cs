namespace Ifolor.ConsumerService.Infrastructure.Messaging
{
    public interface IMessageConsumer
    {
        void StartConsuming();
        Task StopConsumingAsync();
    }
}
