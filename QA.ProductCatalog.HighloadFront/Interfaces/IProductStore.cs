using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Interfaces
{
    public interface IProductStore
    {
        
        Task<string> SearchAsync(ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken = default);
        Task<string> FindByIdAsync(ProductsOptionsBase options, string language, string state);
        Task<string> GetIndicesByName(string language, string state);
        List<string> RetrieveIndexNamesFromIndicesResponse(string indices, string alias);
        Task<string> CreateVersionedIndexAsync(string language, string state);
        Task<string> CreateIndexAsync(string language, string state);
        Task<string[]> GetIndexInAliasAsync(string language, string state);
        Task ReplaceIndexesInAliasAsync(string language, string state, string newIndexName, string[] oldIndexNames, string alias);
        Task DeleteIndexByNameAsync(string language, string state, string indexName);
        Task DeleteIndexAsync(string language, string state);
        Task<SonicResult> CreateAsync(JsonElement product, string language, string state);
        Task<SonicResult> UpdateAsync(JsonElement product, string language, string state);
        Task<SonicResult> DeleteAsync(JsonElement product, string language, string state);
        Task<SonicResult> BulkCreateAsync(IEnumerable<JsonElement> products, string language, string state, string newIndexName);

        Task<bool> Exists(JsonElement product, string language, string state);
        string GetId(JsonElement product);
        string GetType(JsonElement product);
        
        string GetId(JsonObject product);
        string GetType(JsonObject product);

        JsonObject GetDefaultIndexSettings();
    }
}