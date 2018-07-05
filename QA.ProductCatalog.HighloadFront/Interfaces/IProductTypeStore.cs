using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductTypeStore: IProductStore
    {
        Task<Stream> GetProductsInTypeStreamAsync(string type, ProductsOptions options, string language, string state);
        string GetType(JObject product);

    }
}