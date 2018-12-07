using System.IO;
using System.Threading.Tasks;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductSearchStore: IProductStore
    {
        Task<Stream> SearchStreamAsync(ProductsOptions options, string language, string state);
        Task<string> SearchAsync(ProductsOptions options, string language, string state);
    }
}