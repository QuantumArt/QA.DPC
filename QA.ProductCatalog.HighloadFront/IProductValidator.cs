using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductValidator
    {
        Task<SonicResult> ValidateAsync(ProductManager manager, JObject user);
    }


}