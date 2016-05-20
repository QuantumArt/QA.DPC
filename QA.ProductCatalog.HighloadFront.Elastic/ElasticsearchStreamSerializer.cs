using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class ElasticsearchStreamSerializer : IElasticsearchSerializer
    {
        private readonly IElasticsearchSerializer _serializer;

        public ElasticsearchStreamSerializer(IElasticsearchSerializer serializer)
        {
            _serializer = serializer;
        }

        public IPropertyMapping CreatePropertyMapping(MemberInfo memberInfo)
        {
            return _serializer.CreatePropertyMapping(memberInfo);
        }

        public T Deserialize<T>(Stream stream)
        {
            if (typeof(T) == typeof(SearchResponse<StreamData>))
            {
                object res = new StreamResponse(stream);
                return (T)res;
            }
            else
            {
                return _serializer.Deserialize<T>(stream);
            }
        }

        public async Task<T> DeserializeAsync<T>(Stream responseStream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (typeof(T) == typeof(SearchResponse<StreamData>))
            {
                object res = new StreamResponse(responseStream);
                return (T)res;
            }
            else
            {
                return await _serializer.DeserializeAsync<T>(responseStream, cancellationToken);
            }
        }

        public void Serialize(object data, Stream writableStream, SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            _serializer.Serialize(data, writableStream, formatting);
        }
    }
}
