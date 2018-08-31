using System;
using System.Collections.Generic;
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
    [Route("api/mnr")]
    public class InternationalRoamingController : BaseController
    {
        private readonly InternationalRoamingCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;


        public InternationalRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new InternationalRoamingCalculator();
        }

        [HttpGet("country/{countryCode}")]
        public async Task<ActionResult> Get(string countryCode, [FromQuery] bool isB2C = true, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var searchOptions = new SearchOptions
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language)
            };

            ActionResult result = null;
            JObject json = new JObject();
            int[] ids = { 0 };

            try
            {
                searchOptions.TypeName = "RoamingCountry";
                if (int.TryParse(countryCode, out int countryId))
                {
                    json = (await SearchRepo.GetProducts(new[] {countryId}, searchOptions)).First();
                    countryCode = json.SelectToken("Alias").ToString();
                }
                else
                {
                    json = await SearchRepo.GetRoamingCountry(countryCode, searchOptions);
                }

                searchOptions.TypeName = "RoamingScale";
                ids = await SearchRepo.GetRoamingScaleForCountry(countryCode, isB2C, searchOptions);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while getting information for roaming country: {ex.Message}";
                Logger.LogError(1, ex, $"{message}. Address: {searchOptions.BaseAddress}, Index: {searchOptions.IndexName}, Type: {searchOptions.TypeName}");
                result = BadRequest(message);
            }
            searchOptions.TypeName = null;
            result = result ?? await LoadProducts(ids[0], ids.Skip(1).ToArray(), searchOptions, true);
            result = result ?? FilterServicesOnProduct(true);
            if (result != null)
                return result;

            var defaultProduct = (JObject)Product.DeepClone();
            var defaultServices = Services;
            Services = new JObject[] {};
            result = CalculateImpact(countryCode, searchOptions.HomeRegionData);
            var simpleProduct = Product;
            var resultProducts = new List<JObject>();

            foreach (var s in defaultServices)
            {
                if (result == null)
                {
                    Product = (JObject)defaultProduct.DeepClone();
                    Services = new[] { s };
                    result = CalculateImpact(countryCode, searchOptions.HomeRegionData);
                    Product["OptionId"] = s["Id"];
                    resultProducts.Add(Product);
                }

            }

            json["RoamingScaleWithOptions"] = new JArray(resultProducts);
            json["BaseRoamingScale"] = simpleProduct;
            result = result ?? Content(json.ToString());
            return result;

        }

        [HttpGet("option/{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] string countryCode = "WorldExceptRussia", string state = ElasticIndex.DefaultState,
            string language = ElasticIndex.DefaultLanguage, bool html = false)
        {
            var searchOptions = new SearchOptions
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language)
            };
            var serviceIds = new int[] { };
            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, countryCode, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);

            result = result ?? CalculateOption(countryCode);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language, country: countryCode) : Content(Product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string countryCode = "WorldExceptRussia", string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language)
            };

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, countryCode, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);            

            result = result ?? FilterServicesOnProduct(true);

            LogStartImpact("MNR", id, serviceIds);

            result = result ?? CalculateImpact(countryCode, searchOptions.HomeRegionData);

            LogEndImpact("MNR", id, serviceIds);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language, country: countryCode) : Content(Product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;

        }

        private ActionResult CalculateImpact(string countryCode, JObject homeRegionData)
        {
            try
            {
                _calc.Calculate(Product, Services.ToArray(), countryCode, homeRegionData);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating impact: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }

        private ActionResult CalculateOption(string countryCode)
        {
            try
            {
                _calc.Calculate(Product, countryCode);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating option: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }
    }
}
