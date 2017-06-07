using Elasticsearch.Net;

namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    public static class ElasticsearchSerializerExtensions
    {
        public static IElasticsearchSerializer EnableStreamResponse(this IElasticsearchSerializer serializer)
        {
            return new ElasticsearchStreamSerializer(serializer);
        }
    }
}
