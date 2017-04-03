using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mnr")]
    public class InternationalRoamingController : BaseController
    {
        private readonly InternationalRoamingCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;


        public InternationalRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new InternationalRoamingCalculator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string countryCode = "WorldExceptRussia", string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language)
            };

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, countryCode, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await LoadProducts(id, serviceIds, searchOptions);


            LogStartImpact("MNR", id, serviceIds);

            result = result ?? CalculateImpact(countryCode);

            LogEndImpact("MNR", id, serviceIds);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language) : Content(Product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;

        }

        private ActionResult CalculateImpact(string countryCode)
        {
            try
            {
                _calc.Calculate(Product, Services.ToArray(), countryCode);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating impact: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }
    }
}
