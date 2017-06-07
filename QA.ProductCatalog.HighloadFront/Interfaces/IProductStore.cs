using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStore : IDisposable
    {
        
        Task<SonicResult> CreateAsync(JObject product, string language, string state);
        string GetId(JObject product);
        Task<SonicResult> UpdateAsync(JObject product, string language, string state, CancellationToken cancellationToken = default(CancellationToken));
        Task<SonicResult> DeleteAsync(JObject product, string language, string state);
        Task<SonicResult> ResetAsync(string language, string state);
        Task<bool> Exists(JObject product, string language, string state);
    }
}