using QA.Core.DPC.Kafka.API.Models;

namespace QA.Core.DPC.Kafka.API.Interfaces;

public interface IKafkaService
{
    Task<SendResult> SendMessageToKafka(string productId,
        string topic,
        string data,
        string customerCode,
        string method,
        string state,
        string language,
        string format,
        CancellationToken cancellationToken);
}
