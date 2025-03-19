namespace Ifolor.ConsumerService.Infrastructure.Messaging
{
    /// <summary>
    /// Configs setting consumer logic, like maximal connection retry
    /// </summary>
    public class ConsumerPolicyConfig
    {
        public int ResendDelayInSeconds { get; set; } = 30;
        public int MaxConnectionRetry { get; set; } = 5;
        public int DelayBetweenConnectionRetryInSeconds { get; set; } = 5;
    }
}
