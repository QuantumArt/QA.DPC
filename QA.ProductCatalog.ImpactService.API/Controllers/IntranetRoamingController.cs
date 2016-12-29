using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;
using QA.ProductCatalog.ImpactService;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/vsr")]
    public class InRoamingController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        private readonly ConfigurationOptions _configurationOptions;

        public InRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor)
        {
            _searchRepo = searchRepo;
            _configurationOptions = elasticIndexOptionsAccessor.Value;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] int regionId, int homeRegionId, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var allProductIds = new[] {id}.Union(serviceIds).ToArray();
            var searchOptions = new SearchOptions()
            {
                BaseAddress = _configurationOptions.ElasticBaseAddress,
                IndexName = _configurationOptions.GetIndexName(state, language),
                HomeRegionId = homeRegionId
            };

            JObject[] results;
            try
            {
                results = await _searchRepo.GetProducts(allProductIds, searchOptions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while using Elastic Search: {ex.Message}");
            }

            var product = results.FirstOrDefault(n => (int) n["Id"] == id);
            if (product == null)
                return NotFound($"Product {id} is not found");

            var services = new List<JObject>();
            foreach (var serviceId in serviceIds)
            {
                var service = results.FirstOrDefault(m => (int) m["Id"] == serviceId);
                if (service == null)
                    return NotFound($"Service {serviceId} is not found");
                services.Add(service);
            }

            var scaleIds = product.SelectTokens("RoamingScalesOnTariff.[?(@.Id)].RoamingScale.Id").Select(n => (int) n).ToArray();

            JObject[] results2;
            try
            {
                results2 = await _searchRepo.GetProducts(scaleIds, searchOptions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while using Elastic Search: {ex.Message}");
            }

            var scale =
                results2.SingleOrDefault(n => n.SelectTokens("MarketingProduct.Regions.[?(@.Id)].Id").Select(m => (int) m).Contains(regionId)) ??
                results2.SingleOrDefault(n => n.SelectToken("MarketingProduct.Regions") == null);

            if (scale == null)
                return NotFound($"Roaming scale is not found for tariff {id} in regions: {regionId}, {_configurationOptions.RootRegionId}");

            var calc = new IntranetRoamingCalculator();


            var useTariffData = calc.MergeLinkImpactToRoamingScale(scale, product);

            IEnumerable<JToken> parameters = (!useTariffData)
                    ? scale.SelectToken("Parameters")
                    : FilterForRoaming((JArray) product.SelectToken("Parameters"));


            product["Parameters"] = parameters as JArray ?? new JArray(parameters);

            try
            {
                calc.Calculate(product, services.ToArray());
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while calculating impact: {ex.Message}");
            }
            return Content(product.ToString());

        }

        private IEnumerable<JToken> FilterForRoaming(JArray root)
        {
            var allRussiaParams = root.Where(n => n.SelectToken("Zone.Alias")?.ToString() == "Russia").ToArray();
            var vsrParams = root.Where(n => n.SelectToken("Zone.Alias")?.ToString() == "RussiaExceptHome").ToArray();
            var parentParamIds = new HashSet<int>(allRussiaParams.Union(vsrParams)
                .Select(n => n.SelectToken("Parent.Id"))
                .Where(n => n != null)
                .Select(n => (int) n));

            var parentParams = root.Where(n => parentParamIds.Contains((int) n["Id"]) && n["Zone"] == null).ToArray();

            return allRussiaParams.Union(vsrParams).Union(parentParams).ToArray();





        }
    }
}
