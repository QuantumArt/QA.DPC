using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadPostProcessor
    {
        Task<JArray> ReadSourceNodes(string input, ProductsOptionsBase options);
    }
}
