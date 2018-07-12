using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic.Extensions
{
    public static class ElasticClientExtensions
    {
        public static Stream SearchStream(this IElasticClient client, SearchDescriptor<StreamData> request)
        {
            var response =  client.Search<StreamData>(request) as StreamResponse;
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

            selector?.Invoke(request);

            return request;
        }

        public static MappingsDescriptor MapNotAnalyzed(this MappingsDescriptor descriptor, string[] types, string[] fields)
        {
            if (types != null && fields != null && types.Any() && fields.Any())
            {
                foreach (var type in types)
                {
                    descriptor = descriptor.Map(type, m => m.DynamicTemplates(d => {
                        foreach (var field in fields)
                        {
                            d = d.DynamicTemplate($"analyzed_{type}_{field}", t => t.Match(field).MatchMappingType("string").Mapping(mf => mf.Keyword(f => f)));
                        }

                        return d;
                    }));
                }
            }

            return descriptor;
        }

        public static MappingsDescriptor MapAnalyzed(this MappingsDescriptor descriptor, string[] types, string[] fields)
        {
            if (types != null && fields != null && types.Any() && fields.Any())
            {
                foreach (var type in types)
                {
                    descriptor = descriptor.Map(type, m => m.DynamicTemplates(d => {
                        foreach (var field in fields)
                        {
                            d = d.DynamicTemplate($"analyzed_{type}_{field}", t => t.Match(field).MatchMappingType("string").Mapping(mf => mf.Text(f => f)));
                        }

                        d = d.DynamicTemplate($"analyzed_{type}_all", t => t.Match("*").MatchMappingType("string").Mapping(mf => mf.Keyword(f => f)));

                        return d;
                    }));
                }
            }

            return descriptor;
        }           
    }
}