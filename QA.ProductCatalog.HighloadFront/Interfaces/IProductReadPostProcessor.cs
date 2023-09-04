using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadPostProcessor
    {
        Task<string> ReadSourceNodes(string input, ProductsOptionsBase options);
    }
}
