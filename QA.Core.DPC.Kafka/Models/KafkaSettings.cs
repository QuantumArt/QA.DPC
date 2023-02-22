namespace QA.Core.DPC.Kafka.Models
{
    public class KafkaSettings
    {
        public bool IsEnabled { get; set; }
        public int Acks { get; set; } = -1;
        public string? BootstrapServers { get; set; }
        public int MessageSendMaxRetries { get; set; } = int.MaxValue;
        public int RetryBackoffMs { get; set; } = 10_000;
        public int SecurityProtocol { get; set; } = 0;
        public string SaslUsername { get; set; } = string.Empty;
        public string SaslPassword { get; set; } = string.Empty;
    }
}
