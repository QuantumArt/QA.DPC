using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductSearchStore: IProductStore
    {
        Task<IList<JObject>> SearchAsync(string query, ProductsOptions options);
        Task<Stream> SearchStreamAsync(string q, ProductsOptions options);
    }
}