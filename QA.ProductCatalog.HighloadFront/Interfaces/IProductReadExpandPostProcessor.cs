using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadExpandPostProcessor
    {
        int[] GetExpandIdsWithVerification(JToken input, ProductsOptionsExpand options);

        int[] GetExpandIds(JToken expandableNode, ProductsOptionsExpand options);

        int GetId(JToken token);
    }
}
