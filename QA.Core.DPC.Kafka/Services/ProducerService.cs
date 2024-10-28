using Confluent.Kafka;
using Newtonsoft.Json;
using NLog;
using QA.Core.DPC.Kafka.Helpers;
using QA.Core.DPC.Kafka.Interfaces;
using QA.Core.DPC.Kafka.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using ILogger = NLog.ILogger;

namespace QA.Core.DPC.Kafka.Services
{
    public class ProducerService<TKey> : IProducerService<TKey>
    {
        private readonly IProducer<TKey, string> _producer;
        private readonly IAdminClient _adminClient;
        private readonly TimeSpan _timeout;
        private readonly bool _checkTopicExists;
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public ProducerService(KafkaSettings settings)
        {
            _producer = new ProducerBuilder<TKey, string>(settings.GetProducerConfig())
                .SetLogHandler((_, message) =>
                {
                    KafkaHelper.LogSysLogMessage(_logger, message);
                }).Build();
            
            _adminClient = new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = settings.Producer.BootstrapServers
                }).SetLogHandler((_, message) =>
                {
                    KafkaHelper.LogSysLogMessage(_logger, message);
                }).Build();
            

            _timeout = TimeSpan.FromMilliseconds(settings.RequestTimeoutInMs);
            _checkTopicExists = settings.CheckTopicExists;
        }

        public async Task<PersistenceStatus> SendString(TKey key,
            string value,
            string topic,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                _logger.Error("Topic name is empty. Check application configuration.");
                return PersistenceStatus.NotPersisted;
            }

            if (_checkTopicExists)
            {
                var data = _adminClient.GetMetadata(_timeout);
                var exists = data.Topics.Any(n => n.Topic == topic);
                if (!exists)
                {
                    _logger.Error("Topic {topic} does not exist. Check application configuration.", topic);
                    return PersistenceStatus.NotPersisted;                    
                }
            }

            var result = await _producer.ProduceAsync(topic, 
                new Message<TKey, string> { Key = key, Value = value },
                cancellationToken
            );

            return result.Status;
        }

        public async Task<PersistenceStatus> SendSerializedArticle(TKey key,
            Article article,
            string topic,
            IArticleFormatter formatter,
            CancellationToken cancellationToken)
        {
            string serializedArticle = formatter.Serialize(article);

            return await SendString(key, serializedArticle, topic, cancellationToken);
        }

        public async Task<PersistenceStatus> SendAsJson(TKey key,
            object value,
            string topic,
            CancellationToken cancellationToken)
        {
            string jsonValue = JsonConvert.SerializeObject(value);

            return await SendString(key, jsonValue, topic, cancellationToken);
        }

        public void Dispose() => _producer.Dispose();
    }
}
