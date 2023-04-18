using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStore : IDisposable
    {
        
        Task<string> SearchAsync(ProductsOptionsBase options, string language, string state);
        Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state);
        Task<string> FindSourceByIdsAsync(int[] ids, string language, string state);
        Task<string> GetIndicesByName(string language, string state);
        List<string> RetrieveIndexNamesFromIndicesResponse(string indices, string alias);
        Task<string> CreateVersionedIndexAsync(string language, string state);
        Task<string[]> GetIndexInAliasAsync(string language, string state);
        Task ReplaceIndexesInAliasAsync(string language, string state, string newIndexName, string[] oldIndexNames, string alias);
        Task DeleteIndexByNameAsync(string language, string state, string indexName);

        Task<SonicResult> CreateAsync(JObject product, string language, string state);
        Task<SonicResult> UpdateAsync(JObject product, string language, string state);
        Task<SonicResult> DeleteAsync(JObject product, string language, string state);
        Task<SonicResult> BulkCreateAsync(IEnumerable<JObject> products, string language, string state, string newIndexName);

        Task<bool> Exists(JObject product, string language, string state);
        string GetId(JObject product);
        string GetType(JObject product);
    }
}