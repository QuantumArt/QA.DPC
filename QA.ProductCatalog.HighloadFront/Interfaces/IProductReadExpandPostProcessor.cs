using System.Text.Json.Nodes;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadExpandPostProcessor
    {
        int[] GetExpandIdsWithVerification(JsonNode input, ProductsOptionsExpand options);

        int[] GetExpandIds(JsonObject expandableNode, ProductsOptionsExpand options);

        int GetId(JsonObject token);
    }
}
