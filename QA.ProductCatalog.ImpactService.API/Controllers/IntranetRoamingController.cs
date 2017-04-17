using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/vsr")]
    public class InRoamingController : BaseController
    {

        private readonly IntranetRoamingCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        protected JObject Scale;

        protected JArray InitialTariffProperties;


        public InRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new IntranetRoamingCalculator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string region, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {
            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };


            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, region, homeRegion, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await LoadProducts(id, serviceIds, searchOptions);

            var useMacroRegionParameters = await IsOneMacroRegion(region, homeRegion, searchOptions);


            result =  result ?? FilterServicesOnProduct(true);

            LogStartImpact("VSR", id, serviceIds);

            result = result ?? await CorrectProductWithScale(id, region, useMacroRegionParameters, searchOptions);
            result = result ?? CalculateImpact(homeRegion);

            LogEndImpact("VSR", id, serviceIds);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language, homeRegion, region) : Content(Product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }
            return result;
        }

        protected async Task<ActionResult> CorrectProductWithScale(int id, string region, bool useMacroRegionParameters, SearchOptions searchOptions)
        {
            var result = await LoadScale(id, region, searchOptions);
            if (result == null)
            {
                try
                {
                    InitialTariffProperties = (JArray)Product["Parameters"];
                    Product["Parameters"] = _calc.GetResultParameters(Scale, Product, useMacroRegionParameters);
                }
                catch (Exception ex)
                {
                    var message = $"Exception occurs while merging scale and tariff: {ex.Message}";
                    Logger.LogError(1, ex, message);
                    result = BadRequest(message);
                }

            }
            return result;
        }

        private async Task<ActionResult> LoadScale(int id, string region, SearchOptions searchOptions)
        {
            var scaleIds =
                Product.SelectTokens("RoamingScalesOnTariff.[?(@.Id)].RoamingScale.Id").Select(n => (int) n).ToArray();

            JObject[] scales;
            try
            {
                scales = await SearchRepo.GetProducts(scaleIds, searchOptions);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while getting scales: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }

            Scale = _calc.FilterScale(region, scales);

            return Scale == null ? NotFound($"Roaming scale is not found for tariff {id} in regions: {region}, {ConfigurationOptions.RootRegionId}") : null;
        }

        protected override ActionResult CalculateImpact(string homeRegion)
        {
            try
            {
                foreach (var service in Services)
                {
                    _calc.MergeValuesFromTariff(service, InitialTariffProperties);   
                }

                _calc.Calculate(Product, Services.ToArray(), homeRegion);
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
