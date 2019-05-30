using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public interface ISearchRepository
    {
        Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options, DateTimeOffset defaultValue);

        Task<JObject[]> GetProducts(int[] productIds, SearchOptions options);

        Task<bool> IsOneMacroRegion(string[] regions, SearchOptions options);

        Task<int[]> GetRoamingScaleForCountry(string code, bool isB2C, SearchOptions options);

        Task<JObject> GetRoamingCountry(string code, SearchOptions options);

        Task<JObject> GetHomeRegion(SearchOptions options);
        
        Task<string> GetDefaultRegionAliasForMnr(SearchOptions options);

        Task<bool> GetIndexIsTyped(SearchOptions options);
    }
}
