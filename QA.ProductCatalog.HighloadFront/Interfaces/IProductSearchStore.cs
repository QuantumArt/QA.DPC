using System.IO;
using System.Threading.Tasks;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductSearchStore: IProductStore
    {
        Task<Stream> SearchStreamAsync(ProductsOptions options, string language, string state);
    }
}