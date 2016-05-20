using Elasticsearch.Net;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public static class ElasticsearchSerializerExstensions
    {
        public static IElasticsearchSerializer EnableStreamResponse(this IElasticsearchSerializer serializer)
        {
            return new ElasticsearchStreamSerializer(serializer);
        }
    }
}
