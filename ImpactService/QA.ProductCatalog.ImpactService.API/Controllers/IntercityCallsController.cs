using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mg")]
    public class IntercityCallsController : BaseCallsController
    {

        private readonly IntercityCallsCalculator _calc;


        public IntercityCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new IntercityCallsCalculator(ConfigurationOptions.ConsolidateCallGroupsForIcin);
        }

        protected override BaseImpactCalculator Calculator => _calc;

        protected override BaseCallsImpactCalculator CallsCalculator => _calc;


        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string region, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions()
            {
                BaseUrls = ConfigurationOptions.ElasticUrls,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            ConfigureOptions(searchOptions);

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, region, homeRegion, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);

            LogStartImpact("MG", id, serviceIds);

            result = result ?? FilterServiceParameters(region);
            
            result = result ?? FilterProductParameters(region, false, false); // to mark special directions
            result = result ?? CalculateImpact(searchOptions.HomeRegionData);
            result = result ?? FilterProductParameters(region);

            result = result ?? FilterServicesOnProduct(false);
            var product = (result == null) ? GetNewProduct(searchOptions.HomeRegionData) : null;

            LogEndImpact("MG", id, serviceIds);

            result = result ?? (html ? TestLayout(product, serviceIds, state, language, homeRegion, region) : Content(product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;

        }
    }
}
