using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStreamStore
    {
        Task<ElasticsearchResponse<Stream>> FindStreamByIdAsync(ProductsOptions options, string language, string state);
    }
}