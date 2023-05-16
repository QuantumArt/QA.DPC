using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadExpandPostProcessor
    {
        int[] GetExpandIds(JToken input, ProductsOptionsExpand options);
    }
}
