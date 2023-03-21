using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStore : IDisposable
    {
        
        Task<string> SearchAsync(ProductsOptions options, string language, string state);
        Task<string> FindByIdAsync(ProductsOptions options, string language, string state);
        Task<string> FindSourceByIdsAsync(int[] ids, string language, string state);

        Task<SonicResult> CreateAsync(JObject product, string language, string state);
        Task<SonicResult> UpdateAsync(JObject product, string language, string state);
        Task<SonicResult> DeleteAsync(JObject product, string language, string state);
        Task<SonicResult> ResetAsync(string language, string state);
        Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> product, string language, string state);

        Task<bool> Exists(JObject product, string language, string state);
        string GetId(JObject product);
        string GetType(JObject product);
    }
}