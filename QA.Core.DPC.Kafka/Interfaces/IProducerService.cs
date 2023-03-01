using Confluent.Kafka;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Kafka.Interfaces
{
    public interface IProducerService<TKey> : IDisposable
    {
        /// <summary>
        /// Send string message to kafka. You must build string in correct format before calling this method.
        /// </summary>
        /// <param name="key">Key of the message</param>
        /// <param name="value">String value to send</param>
        /// <param name="topic">Topic name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Message persistence status</returns>
        Task<PersistenceStatus> SendString(TKey key, string value, string topic, CancellationToken cancellationToken);

        /// <summary>
        /// Send article to kafka. Article will be serialized using IArticleFormatter from unity container.
        /// </summary>
        /// <param name="key">Key of the message</param>
        /// <param name="article">Article to serialize and send</param>
        /// <param name="topic">Topic name</param>
        /// <param name="formatter">IArticleFormatter instance</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Message persistence status</returns>
        Task<PersistenceStatus> SendSerializedArticle(TKey key, Article article, string topic, IArticleFormatter formatter, CancellationToken cancellationToken);

        /// <summary>
        /// Send object to kafka. Object will be serialized to json using newtonsoft.json default serializer.
        /// </summary>
        /// <param name="key">Key of the message</param>
        /// <param name="value">Object to send</param>
        /// <param name="topic">Topic name</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Message persistence status</returns>
        Task<PersistenceStatus> SendAsJson(TKey key, object value, string topic, CancellationToken cancellationToken);
    }
}
