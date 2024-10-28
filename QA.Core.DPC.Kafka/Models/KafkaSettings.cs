using Confluent.Kafka;

namespace QA.Core.DPC.Kafka.Models
{
    public class KafkaSettings
    {
        public string TopicName { get; set; } = string.Empty;
        
        public bool CheckTopicExists { get; set; }

        public ProducerConfig Producer { get; set; } = new();

        public int RequestTimeoutInMs => Producer.RequestTimeoutMs ?? 30_000;

        public Dictionary<string, string> GetProducerConfig()
        {
            var filtered = Producer
                .Where(n => !string.IsNullOrEmpty(n.Value))
                .ToDictionary(n => n.Key, n => n.Value);
            return filtered;
        }
    }
}