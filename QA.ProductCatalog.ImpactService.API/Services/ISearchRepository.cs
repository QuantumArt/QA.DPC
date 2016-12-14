using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.ImpactService.API.Services
{
    public interface ISearchRepository
    {
        Task<DateTimeOffset> GetLastUpdated(int[] productIds);

        Task<JObject[]> GetProducts(int[] productIds);


    }
}
