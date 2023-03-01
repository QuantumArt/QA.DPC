using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QA.Core.DPC.Kafka.Interfaces;
using QA.Core.DPC.Kafka.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Kafka.Services
{
    public class ProducerService<TKey> : IProducerService<TKey>
    {
        private readonly IProducer<TKey, string> _producer;
        private readonly ILogger<ProducerService<TKey>> _logger;

        public ProducerService(IOptions<KafkaSettings> settings, ILogger<ProducerService<TKey>> logger)
        {
            ProducerConfig config = new()
            {
                Acks = (Acks)settings.Value.Acks,
                BootstrapServers = settings.Value.BootstrapServers,
                MessageSendMaxRetries = settings.Value.MessageSendMaxRetries,
                RetryBackoffMs = settings.Value.RetryBackoffMs,
                SecurityProtocol = (SecurityProtocol)settings.Value.SecurityProtocol
            };

            if (config.SecurityProtocol is SecurityProtocol.SaslPlaintext or SecurityProtocol.SaslSsl)
            {
                config.SaslUsername = settings.Value.SaslUsername;
                config.SaslPassword = settings.Value.SaslPassword;
            }

            _producer = new ProducerBuilder<TKey, string>(config).Build();
            _logger = logger;
        }

        public async Task<PersistenceStatus> SendString(TKey key, string value, string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                _logger.LogError("Kafka topic is empty. Check application configuration.");

                return PersistenceStatus.NotPersisted;
            }

            DeliveryResult<TKey, string>? result = await _producer.ProduceAsync(topic, new Message<TKey, string>() { Key = key, Value = value });

            return result.Status;
        }

        public async Task<PersistenceStatus> SendSerializedArticle(TKey key, Article article, string topic, IArticleFormatter formatter)
        {
            string serializedArticle = formatter.Serialize(article);

            return await SendString(key, serializedArticle, topic);
        }

        public async Task<PersistenceStatus> SendAsJson(TKey key, object value, string topic)
        {
            string jsonValue = JsonConvert.SerializeObject(value);

            return await SendString(key, jsonValue, topic);
        }

        public void Dispose() => _producer.Dispose();
    }
}
