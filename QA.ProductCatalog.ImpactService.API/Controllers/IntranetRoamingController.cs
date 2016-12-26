using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;
using QA.ProductCatalog.ImpactService;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/vsr")]
    public class InRoamingController : Controller
    {
        private readonly ISearchRepository _searchRepo;

        public InRoamingController(ISearchRepository searchRepo)
        {
            _searchRepo = searchRepo;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] int regionId)
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

            try
            {
                new IntranetRoamingCalculator().Calculate(product, services.ToArray());
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while calculating impact: {ex.Message}");
            }
            return Content(product.ToString());

        }
    }
}
