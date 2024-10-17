using Confluent.Kafka;

namespace QA.Core.DPC.Kafka.Models
{
    public class KafkaSettings
    {
        public string TopicName { get; set; } = string.Empty;
        
        public bool CheckTopicExists { get; set; }
    }
}
