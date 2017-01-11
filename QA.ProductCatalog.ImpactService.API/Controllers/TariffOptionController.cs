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
    [Route("api/base")]
    public class TariffOptionController : BaseController
    {

        private readonly TariffOptionCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        public TariffOptionController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory)
        {
            _calc = new TariffOptionCalculator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            var result = await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? CalculateImpact();
            result = result ?? Content(Product.ToString());
            return result;

        }
    }
}
