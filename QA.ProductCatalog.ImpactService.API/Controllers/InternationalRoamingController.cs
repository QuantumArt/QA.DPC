using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mnr")]
    public class InternationalRoamingController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        private readonly ConfigurationOptions _configurationOptions;

        public InternationalRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor)
        {
            _searchRepo = searchRepo;
            _configurationOptions = elasticIndexOptionsAccessor.Value;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string country = "WorldExceptRussia", string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var allProductIds = new[] {id}.Union(serviceIds).ToArray();
            var searchOptions = new SearchOptions()
            {
                BaseAddress = _configurationOptions.ElasticBaseAddress,
                IndexName = _configurationOptions.GetIndexName(state, language)
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


            var services = new List<JObject>();
            foreach (var serviceId in serviceIds)
            {
                var service = results.FirstOrDefault(m => (int)m["Id"] == serviceId);
                if (service == null)
                    return NotFound($"Service {serviceId} is not found");
                services.Add(service);
            }

            try
            {
                new InternationalRoamingCalculator().Calculate(product, services.ToArray(), country);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while calculating impact: {ex.Message}");
            }
            return Content(  product.ToString()) ;

        }

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
