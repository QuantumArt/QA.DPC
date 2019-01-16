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

        private JObject Tariff { get; set; }


        public InternationalRoamingController(
            ISearchRepository searchRepo,
            IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, 
            ILoggerFactory loggerFactory,
            IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new InternationalRoamingCalculator();
        }

        [HttpGet("country/{countryCode}")]
        public async Task<ActionResult> Get(
            string countryCode, 
            [FromQuery] string homeRegion,
            [FromQuery] bool isB2C = true,
            [FromQuery] bool calculateImpact = false,
            string state = ElasticIndex.DefaultState,
            string language = ElasticIndex.DefaultLanguage)
        {

            var searchOptions = new SearchOptions
            {
                BaseUrls = ConfigurationOptions.ElasticUrls,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            JObject json = new JObject();
            int[] ids = { 0 };
            
            var serviceIds = new int[] { };
            var id = Convert.ToInt32(isB2C) * 2 + Convert.ToInt32(calculateImpact);
            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, homeRegion, state, language);
            var result = (!IsCacheDisabled()) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;
            
            if (string.IsNullOrEmpty(searchOptions.HomeRegion))
            {
                searchOptions.HomeRegion = await GetDefaultRegionAliasForMnr(searchOptions);                
            }

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
                searchOptions.TypeName = null;
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while getting information for roaming country: {ex.Message}";
                LogException(ex, message, searchOptions);
                result = BadRequest(message);
            }

            result = result ?? await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(ids[0], ids.Skip(1).ToArray(), searchOptions, true);
            result = result ?? FilterServicesOnProduct(true, null, searchOptions.HomeRegionData);
            
            if (result != null)
                return result;

            var defaultProduct = (JObject)Product.DeepClone();

            var servicesCopy = Services;
            Services = new JObject[] {};
            result = CalculateImpact(countryCode, searchOptions.HomeRegionData);
            json["BaseRoamingScale"] = Product;

            if (calculateImpact)
            {
                Services = servicesCopy;
                result = result ?? AppendImpactNode(countryCode, defaultProduct, searchOptions, json);               
            }

            result = result ?? Content(json.ToString());
            
            if (!IsCacheDisabled())
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }
            
            return result;

        }

        private ActionResult AppendImpactNode(
            string countryCode, 
            JObject defaultProduct, 
            SearchOptions searchOptions, 
            JObject json)
        {
            ActionResult result = null;
            var services = Services;
            var resultProducts = new List<JObject>();

            foreach (var s in services)
            {
                if (result == null)
                {
                    Product = (JObject) defaultProduct.DeepClone();
                    Services = new[] {s};
                    result = CalculateImpact(countryCode, searchOptions.HomeRegionData);
                    Product["OptionId"] = s["Id"];
                    resultProducts.Add(Product);
                }
            }

            json["RoamingScaleWithOptions"] = new JArray(resultProducts);
            return result;
        }

        [HttpGet("option/{id}")]
        public async Task<ActionResult> Get(
            int id, 
            [FromQuery] string homeRegion,
            [FromQuery] string countryCode = "WorldExceptRussia", 
            string state = ElasticIndex.DefaultState,
            string language = ElasticIndex.DefaultLanguage, 
            bool html = false)
        {
            var searchOptions = new SearchOptions
            {
                BaseUrls = ConfigurationOptions.ElasticUrls,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };
            
            if (string.IsNullOrEmpty(searchOptions.HomeRegion))
            {
                searchOptions.HomeRegion = await GetDefaultRegionAliasForMnr(searchOptions);                
            }

            var serviceIds = new int[] { };
            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, homeRegion, state, language);
            var result = (!IsCacheDisabled(html)) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);

            result = result ?? CalculateOption(countryCode);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language, country: countryCode) : Content(Product.ToString()));

            if (!IsCacheDisabled(html))
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;
        }

        private bool IsCacheDisabled(bool html = false)
        {
            return html || ConfigurationOptions.CachingInterval <= 0;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(
            int id, 
            [FromQuery] int[] serviceIds,
            [FromQuery] int? tariffId,
            [FromQuery] string homeRegion,
            [FromQuery] string countryCode = "WorldExceptRussia", 
            string state = ElasticIndex.DefaultState,
            string language = ElasticIndex.DefaultLanguage, 
            bool html = false)
        {

            var searchOptions = new SearchOptions
            {
                BaseUrls = ConfigurationOptions.ElasticUrls,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };
            
            if (string.IsNullOrEmpty(searchOptions.HomeRegion))
            {
                searchOptions.HomeRegion = await GetDefaultRegionAliasForMnr(searchOptions);                
            }

            if (tariffId.HasValue)
            {
                serviceIds = serviceIds.Union(new[] {tariffId.Value}).ToArray();
            }
            
            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, countryCode, homeRegion, state, language);
            var result = (!IsCacheDisabled(html)) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;

            result = await FillHomeRegion(searchOptions);
            result = result ?? await LoadProducts(id, serviceIds, searchOptions);
            SplitLoadedServicesAndTariff(tariffId);
            
            result = result ?? await FillDefaultHomeRegion(searchOptions, Product);
            int[] excluded = Calculator.FindServicesOnProduct(Product, "ServicesOnRoamingScale", "HideInInternationalRoamingCalculatorByDefault");
            if (excluded.Any())
            {
                int[] includedByTariff = Calculator.FindServicesOnProduct(Tariff, "ServicesOnTariff", "ShowInInternationalRoamingCalculator");
                excluded = excluded.Where(n => !includedByTariff.Contains(n)).ToArray();
            }
            excluded = excluded.Union(
                Calculator.FindServicesOnProduct(Tariff, "ServicesOnTariff", "HideInInternationalRoamingCalculator")
            ).ToArray();
            
            result = result ?? FilterServicesOnProduct(true, excluded, searchOptions.HomeRegionData);

            LogStartImpact("MNR", id, serviceIds);

            result = result ?? CalculateImpact(countryCode, searchOptions.HomeRegionData);

            LogEndImpact("MNR", id, serviceIds);

            if (html)
            {
                result = result ?? TestLayout(
                             Product, 
                             serviceIds, 
                             state, 
                             language,
                             homeRegion: homeRegion,
                             country: countryCode,
                             tariffId: tariffId?.ToString()
                         );
            }
            else
            {
                result = result ?? Content(Product.ToString());
            }
            

            if (!IsCacheDisabled(html))
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;

        }

        private int[] FindServicesOnTariff(JObject tariff, string link, string modifier)
        {
            return tariff?.SelectTokens($"{link}.[?(@.Service)]")
                .Where(n => n.SelectTokens($"Parent.Modifiers.[?(@.Alias == '{modifier}')]").Any())
                .Select(n => (int) n.SelectToken("Service.Id"))
                .ToArray();
        }

        private void SplitLoadedServicesAndTariff(int? tariffId)
        {
            if (tariffId.HasValue)
            {
                Tariff = Services.SingleOrDefault(n => (int) n.SelectToken("Id") == tariffId.Value);
                if (Tariff != null)
                {
                    Services = Services.Where(n => (int) n.SelectToken("Id") != tariffId.Value).ToArray();
                }
            }
        }

        private ActionResult CalculateImpact(string countryCode, JObject homeRegionData)
        {
            try
            {
                _calc.Calculate(Product, Tariff, Services.ToArray(), countryCode, homeRegionData);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating impact: {ex.Message}";
                LogException(ex, message, null);
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
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }
    }
}
