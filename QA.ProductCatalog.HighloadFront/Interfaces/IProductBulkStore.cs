using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductBulkStore
    {
        Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> product, string language, string state);
    }
}