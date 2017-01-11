using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/mn")]
    public class InternationalCallsController : BaseCallsController
    {

        private readonly InternationalCallsCalclulator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        protected override BaseCallsImpactCalculator CallsCalculator => _calc;


        public InternationalCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory)
        {
            _calc = new InternationalCallsCalclulator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string countryCode,
            string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {


            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            var result = await LoadProducts(id, serviceIds, searchOptions);

            result = result ?? FilterServiceParameters(countryCode);

            result = result ?? CalculateImpact();

            result = result ?? FilterProductParameters(countryCode);

            result = result ?? FilterServicesOnTariff();

            result = result ?? GetNewProduct();

            return result;

        }

    }
}
