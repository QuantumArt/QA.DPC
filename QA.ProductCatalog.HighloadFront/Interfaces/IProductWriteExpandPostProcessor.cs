using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductWriteExpandPostProcessor
    {
        void WriteExtraNodes(JToken input, JArray extraNodes, ProductsOptionsExpand options);
    }
}
