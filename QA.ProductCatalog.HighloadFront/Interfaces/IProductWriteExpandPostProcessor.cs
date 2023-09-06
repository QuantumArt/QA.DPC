using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductWriteExpandPostProcessor
    {
        void WriteExtraNodes(JsonNode input, JsonArray extraNodes, ProductsOptionsExpand options);
    }
}
