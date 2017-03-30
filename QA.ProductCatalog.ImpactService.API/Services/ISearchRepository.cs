using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public interface ISearchRepository
    {
        Task<DateTimeOffset> GetLastUpdated(int[] productIds, SearchOptions options, DateTimeOffset defaultValue);

        Task<JObject[]> GetProducts(int[] productIds, SearchOptions options);

    }
}
