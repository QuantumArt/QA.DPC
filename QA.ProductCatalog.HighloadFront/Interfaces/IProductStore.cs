using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStore : IDisposable
    {
        
        Task<string> SearchAsync(ProductsOptions options, string language, string state);
        Task<string> FindByIdAsync(ProductsOptions options, string language, string state);
        Task<string[]> GetIndexesAsync(string language, string state);
        Task<string> CreateVersionedIndexAsync(string language, string state);
        Task<string[]> GetIndexInAliasAsync(string language, string state);
        Task AddIndexToAliasAsync(string language, string state, string newIndexName, string[] oldIndexes, string alias);
        Task DeleteIndexByNameAsync(string language, string state, string indexName);

        Task<SonicResult> CreateAsync(JObject product, string language, string state);
        Task<SonicResult> UpdateAsync(JObject product, string language, string state);
        Task<SonicResult> DeleteAsync(JObject product, string language, string state);
        Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> products, string language, string state, string newIndex);

        Task<bool> Exists(JObject product, string language, string state);
        string GetId(JObject product);
        string GetType(JObject product);
    }
}