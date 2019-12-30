using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NLog;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mn")]
    public class InternationalCallsController : BaseCallsController
    {

        private readonly InternationalCallsCalclulator _calc;

        protected override Logger Logger { get; } = LogManager.GetCurrentClassLogger();
        protected override BaseImpactCalculator Calculator => _calc;

        protected override BaseCallsImpactCalculator CallsCalculator => _calc;

        private IOptions<ConfigurationOptions> _elasticIndexOptionsAccessor;


        public int[] PreCalcServiceIds { get; private set; }

        public List<int> NoImpactServiceIds { get; private set; }


        private InternationalCallsController GetAlternateController()
        {
            return new InternationalCallsController(SearchRepo, _elasticIndexOptionsAccessor, Cache);
        }

        public InternationalCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, cache)
        {
            _elasticIndexOptionsAccessor = elasticIndexOptionsAccessor;
            _calc = new InternationalCallsCalclulator(ConfigurationOptions.ConsolidateCallGroupsForIcin);
            PreCalcServiceIds = new int[0];
            NoImpactServiceIds = new List<int>();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string countryCode,
            string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {
            return await InternalGet(id, serviceIds, countryCode, homeRegion, state, language, html);
        }

        private async Task<ActionResult> InternalGet(int id, int[] serviceIds, string countryCode, string homeRegion, string state,
            string language, bool html, bool initial = true)
        {
            var searchOptions = new SearchOptions()
            {
                BaseUrls = ConfigurationOptions.ElasticUrls,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            ConfigureOptions(searchOptions);

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, homeRegion, state, language);
            var result = await GetCachedResult(cacheKey, searchOptions, html);
            
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);            

            if (initial)
            {
                LogStartImpact("MN", id, serviceIds);
            }

            if (initial)
            {
                result = result ?? FindPreCalcServices();
                result = result ?? await FindNoImpactServices(id, countryCode, homeRegion, state, language, html);
            }

            result = result ?? FilterServiceParameters(countryCode);
            
            result = result ?? FilterProductParameters(countryCode, false, false); // to mark special directions
            result = result ?? CalculateImpact(searchOptions.HomeRegionData);
            result = result ?? FilterProductParameters(countryCode);                 
            
            result = result ?? FilterServicesOnProduct(false, NoImpactServiceIds);

            var product = (result == null) ? GetNewProduct(searchOptions.HomeRegionData) : null;

            if (initial)
            {
                LogEndImpact("MN", id, serviceIds);
            }

            if (initial)
            {
                result = result ?? (html
                             ? TestLayout(product, serviceIds, state, language, homeRegion, country: countryCode)
                             : Content(product.ToString()));
            }
            else
            {
                result = result ?? Content(product.ToString());
            }

            if (!IsCacheDisabled(html))
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;
        }

        private async Task<ActionResult> FindNoImpactServices(int id, string countryCode, string homeRegion, string state, string language,
            bool html)
        {
            try
            {
                var precalcIds = PreCalcServiceIds.Where(n => !ServiceIds.Contains(n)).ToArray();
                foreach (var precalcId in precalcIds)
                {
                    var controller = GetAlternateController();
                    await controller.InternalGet(
                        id, new[] { precalcId }, countryCode, homeRegion, state, language, html,
                        false);
                    if (!controller.HasImpactForDirections() )
                    {
                        NoImpactServiceIds.Add(precalcId);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while searching for no-impact services: {ex.Message}";
                LogException(ex, message, null);
                return BadRequest(message);
            }

            return null;
        }


        private bool HasImpactForDirections()
        {
            return CallsCalculator.HasImpactForDirections(Parameters);
        }

        private ActionResult FindPreCalcServices()
        {
            try
            {
                PreCalcServiceIds = CallsCalculator.GetPreCalcServiceIds(Product).ToArray();
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while precalculating services: {ex.Message}";
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }

    }
}
