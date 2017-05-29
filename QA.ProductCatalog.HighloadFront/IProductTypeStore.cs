using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductTypeStore: IProductStore
    {
        Task<Stream> GetProductsInTypeStreamAsync(string type, ProductsOptions options, string language, string state);
        string GetType(JObject product);

    }
}