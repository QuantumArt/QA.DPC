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
        private readonly IAdminClient _adminClient;
        private readonly TimeSpan _timeout;
        private readonly bool _checkTopicExists;

        public ProducerService(
            IOptions<ProducerConfig> config, 
            IOptions<KafkaSettings> settings, 
            ILogger<ProducerService<TKey>> logger
        )
        {
            _producer = new ProducerBuilder<TKey, string>(config.Value)
                .SetLogHandler((_, message) =>
                {
                    LogSysLogMessage(logger, message);
                }).Build();
            
            _adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = config.Value.BootstrapServers
            }).Build();
            

            _timeout = TimeSpan.FromMilliseconds(config.Value.RequestTimeoutMs ?? 30_000);
            _checkTopicExists = settings.Value.CheckTopicExists;
            _logger = logger;
        }

        private static void LogSysLogMessage(ILogger<ProducerService<TKey>> logger, LogMessage message)
        {
            switch (message.Level)
            {
                case SyslogLevel.Alert:
                case SyslogLevel.Emergency:
                case SyslogLevel.Critical:
                    logger.LogCritical(message.Message);
                    break;
                case SyslogLevel.Error:
                    logger.LogError(message.Message);
                    break;
                case SyslogLevel.Warning:
                    logger.LogWarning(message.Message);
                    break;
                case SyslogLevel.Info:
                case SyslogLevel.Notice:
                    logger.LogInformation(message.Message);
                    break;
                case SyslogLevel.Debug:
                    logger.LogDebug(message.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<PersistenceStatus> SendString(TKey key,
            string value,
            string topic,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                _logger.LogError("Topic name is empty. Check application configuration.");

                return PersistenceStatus.NotPersisted;
            }

            if (_checkTopicExists)
            {
                var data = _adminClient.GetMetadata(_timeout);
                var exists = data.Topics.Any(n => n.Topic == topic);
                if (!exists)
                {
                    _logger.LogError("Topic {topic} does not exist. Check application configuration.", topic);
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
