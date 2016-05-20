using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront
{
    public interface IProductStore : IDisposable
    {
        
        Task<SonicResult> CreateAsync(JObject product);
        string GetId(JObject product);
        Task<SonicResult> UpdateAsync(JObject product, CancellationToken cancellationToken = default(CancellationToken));
        Task<SonicResult> DeleteAsync(JObject product);
        Task<SonicResult> ResetAsync();
    }
}