using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductStreamStore
    {
        Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(string id, ProductsOptions options, string language, string state);
    }
}