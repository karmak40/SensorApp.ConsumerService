using Prometheus;

namespace Ifolor.ConsumerService.Application.Metric
{
    /// <summary>
    /// Metrics to display in prometheus / grafana
    /// </summary>
    public class ConsumerMetrics
    {
        public static readonly Counter MessagesConsumed = Metrics
        .CreateCounter("messages_consumed_total", "Total number of messages consumed.");

        public static readonly Counter MessagesProcessed = Metrics
            .CreateCounter("messages_processed_total", "Total number of messages successfully processed.");

        public static readonly Counter MessagesFailed = Metrics
            .CreateCounter("messages_failed_total", "Total number of messages that failed processing.");

        public static readonly Histogram ProcessingLatency = Metrics
            .CreateHistogram("message_processing_latency_seconds", "Latency of message processing in seconds.");

        public static readonly Gauge MessagesInQueue = Metrics
            .CreateGauge("rabbitmq_messages_in_queue", "Current number of messages in the processing queue.");

        public static readonly Histogram MessageProcessingDuration = Metrics.CreateHistogram(
              "rabbitmq_message_processing_duration_seconds",
              "Histogram of message processing durations.",
              new HistogramConfiguration
              {
                  Buckets = Histogram.LinearBuckets(start: 0.1, width: 0.2, count: 5)
              });

    }
}
