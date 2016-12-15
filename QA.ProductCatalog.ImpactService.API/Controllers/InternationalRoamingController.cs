using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mnr")]
    public class InternationalRoamingController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        public InternationalRoamingController(ISearchRepository searchRepo)
        {
            _searchRepo = searchRepo;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, string country = "WorldExceptRussia")
        {

            var a = new[] {id}.Union(serviceIds).ToArray();
            JObject[] results;
            try
            {
                results = await _searchRepo.GetProducts(a);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while using Elastic Search: {ex.Message}");
            }
            var product = results.FirstOrDefault(n => (int)n["Id"] == id);
            if (product == null)
                return NotFound($"Product {id} is not found");

            var calc = new InternationalRoamingCalculator();
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
                calc.Calculate(product, services.ToArray(), country);
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
