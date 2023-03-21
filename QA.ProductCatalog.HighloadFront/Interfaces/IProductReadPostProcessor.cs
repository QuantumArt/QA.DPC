using QA.ProductCatalog.HighloadFront.Options;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductReadPostProcessor
    {
        Task<string> GetResult(string input, ProductsOptions options);
        string Expand(string input, string expanded, ProductsOptions options);
        int[] GetExpandIds(string input, ProductsOptions options);
    }
}
