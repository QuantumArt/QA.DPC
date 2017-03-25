using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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


        public InternationalRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory)
        {
            _calc = new InternationalRoamingCalculator();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string country = "WorldExceptRussia", string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language)
            };

            var result = await LoadProducts(id, serviceIds, searchOptions);

            result = result ?? CalculateImpact(country);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language) : Content(Product.ToString()));
            return result;

        }

        private ActionResult CalculateImpact(string country)
        {
            try
            {
                _calc.Calculate(Product, Services.ToArray(), country);
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
