using Confluent.Kafka;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Models;
using QA.Core.DPC.Kafka.Interfaces;

namespace QA.Core.DPC.Kafka.API.Services;

public class KafkaService : IKafkaService
{
    private readonly IProducerService<string> _producer;
    private readonly IMessageModifierFactory _messageModifierFactory;
    
    public KafkaService(IProducerService<string> producer, IMessageModifierFactory messageModifierFactory)
    {
        _producer = producer;
        _messageModifierFactory = messageModifierFactory;
    }

    public async Task<SendResult> SendMessageToKafka(string productId,
        string topic,
        string data,
        string customerCode,
        string method,
        string state,
        string language,
        string format,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                throw new InvalidOperationException("Product id cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(data))
            {
                throw new InvalidOperationException("Message data is empty.");
            }

            if (string.IsNullOrWhiteSpace(customerCode))
            {
                throw new InvalidOperationException("Unable to build topic name without customer code.");
            }

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException("Unable to build topic name without state.");
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new InvalidOperationException("Unable to build topic name without language");
            }

            topic = topic.Replace("{customerCode}", customerCode);
            topic = topic.Replace("{language}", language);
            topic = topic.Replace("{state}", state);

            IMessageModifier messageModifier = _messageModifierFactory.Build(format);
            string modifiedMessage = messageModifier.AddMethodToMessage(data, method);
            PersistenceStatus result = await _producer.SendString(productId, modifiedMessage, topic, cancellationToken);
            
            ProcessPersistenceStatus(result);

            return new()
            {
                IsSuccess = true,
                Message = InternalSettings.SuccessMessage
            };
        }
        catch (Exception e)
        {
            return new()
            {
                IsSuccess = false,
                Message = e.Message
            };
        }
    }

    private static void ProcessPersistenceStatus(PersistenceStatus status)
    {
        switch (status)
        {
            case PersistenceStatus.Persisted:
                return;
            case PersistenceStatus.NotPersisted:
                throw new InvalidOperationException("Unable to send message to kafka.");
            case PersistenceStatus.PossiblyPersisted:
                throw new InvalidOperationException("Message was sent, but may not be persisted in topic. Please retry.");
            default:
                throw new ArgumentOutOfRangeException(nameof(status),"Unknown persistence status.");
        }
    }
}
