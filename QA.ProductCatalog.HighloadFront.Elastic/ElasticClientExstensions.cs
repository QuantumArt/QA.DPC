using System;
using System.IO;
using System.Threading.Tasks;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public static class ElasticClientExstensions
    {
        public static Stream SearchStream(this IElasticClient client, SearchDescriptor<StreamData> request)
        {
            var response = client.Search<StreamData>(request) as StreamResponse;
            return response?.Stream;
        }

        public static async Task<Stream> SearchStreamAsync(this IElasticClient client, SearchDescriptor<StreamData> request)
        {
            var response = await client.SearchAsync<StreamData>(request) as StreamResponse;
            return response?.Stream;
        }

        public static Stream SearchStream(this IElasticClient client, Func<SearchDescriptor<StreamData>, SearchDescriptor<StreamData>> selector = null)
        {
            var request = GetRequest(selector);
            return client.SearchStream(request);
        }

        public static async Task<Stream> SearchStreamAsync(this IElasticClient client, Func<SearchDescriptor<StreamData>, SearchDescriptor<StreamData>> selector = null)
        {
            var request = GetRequest(selector);
            return await client.SearchStreamAsync(request);
        }

        private static SearchDescriptor<StreamData> GetRequest(Func<SearchDescriptor<StreamData>, SearchDescriptor<StreamData>> selector)
        {
            var request = new SearchDescriptor<StreamData>();

            if (selector != null)
            {
                selector(request);
            }

            return request;
        }
    }
}
