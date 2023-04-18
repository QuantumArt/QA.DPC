using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadPostProcessor
    {
        Task<string> GetResult(string input, ProductsOptionsBase options);
        string Expand(string input, JArray expanded, ProductsOptionsExpand options);
        int[] GetExpandIds(string input, ProductsOptionsExpand options);
        JArray GetExpand(string input);
    }
}
