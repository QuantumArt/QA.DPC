using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mn")]
    public class InternationalCallsController : BaseCallsController
    {

        private readonly InternationalCallsCalclulator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        protected override BaseCallsImpactCalculator CallsCalculator => _calc;


        public InternationalCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new InternationalCallsCalclulator(ConfigurationOptions.ConsolidateCallGroupsForIcin);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string countryCode,
            string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, homeRegion, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await LoadProducts(id, serviceIds, searchOptions);

            LogStartImpact("MN", id, serviceIds);

            result = result ?? FilterServiceParameters(countryCode);
            result = result ?? CalculateImpact(homeRegion);
            result = result ?? FilterProductParameters(countryCode);
            result = result ?? FilterServicesOnProduct();

            var product = (result == null) ? GetNewProduct(homeRegion) : null;

            LogEndImpact("MN", id, serviceIds);

            result = result ?? (html ? TestLayout(product, serviceIds, state, language, homeRegion, country: countryCode) : Content(product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;

        }

    }
}
