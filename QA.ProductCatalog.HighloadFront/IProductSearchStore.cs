using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductSearchStore: IProductStore
    {
        Task<Stream> SearchStreamAsync(ProductsOptions options, string language, string state);
    }
}