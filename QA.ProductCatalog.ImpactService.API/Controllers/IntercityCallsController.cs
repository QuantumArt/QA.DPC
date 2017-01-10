using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mg")]
    public class IntercityCallsController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        private readonly ConfigurationOptions _configurationOptions;

        public IntercityCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor)
        {
            _searchRepo = searchRepo;
            _configurationOptions = elasticIndexOptionsAccessor.Value;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string region, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var allProductIds = new[] { id }.Union(serviceIds).ToArray();
            var searchOptions = new SearchOptions()
            {
                BaseAddress = _configurationOptions.ElasticBaseAddress,
                IndexName = _configurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
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

            var product = results.FirstOrDefault(n => (int)n["Id"] == id);
            if (product == null)
                return NotFound($"Product {id} is not found");

            var servicesList = new List<JObject>();
            foreach (var serviceId in serviceIds)
            {
                var service = results.FirstOrDefault(m => (int)m["Id"] == serviceId);
                if (service == null)
                    return NotFound($"Service {serviceId} is not found");
                servicesList.Add(service);
            }
            var services = servicesList.ToArray();


            var calc = new IntercityCallsCalculator();
            IEnumerable<JToken> parameters;
            IEnumerable<JToken> servicesOnTariff;

            try
            {
                calc.FilterServicesParameters(services, region);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while filtering parameters: {ex.Message}");
            }



            try
            {
                calc.Calculate(product, services);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while calculating impact: {ex.Message}");
            }


            try
            {
                parameters = calc.FilterProductParameters((JArray)product.SelectToken("Parameters"), region);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while filtering parameters: {ex.Message}");
            }

            try
            {
                servicesOnTariff = calc.FilterServicesOnTariff((JArray)product.SelectToken("ServicesOnTariff"));
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while filtering services: {ex.Message}");
            }



            var newProduct = new JObject(new JProperty("Parameters", new JArray(parameters)), new JProperty("ServicesOnTariff", servicesOnTariff));
            calc.Reorder(newProduct);

            return Content(newProduct.ToString());

        }


    }
}
