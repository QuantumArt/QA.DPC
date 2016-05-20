using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductBulkStore
    {
        Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> product);
    }
}