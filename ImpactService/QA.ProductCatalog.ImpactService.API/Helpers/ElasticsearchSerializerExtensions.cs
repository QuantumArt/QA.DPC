using Elasticsearch.Net;

namespace QA.ProductCatalog.ImpactService.API.Helpers
{
    public static class ElasticsearchSerializerExtensions
    {
        public static IElasticsearchSerializer EnableStreamResponse(this IElasticsearchSerializer serializer)
        {
            return new ElasticsearchStreamSerializer(serializer);
        }
    }
}
