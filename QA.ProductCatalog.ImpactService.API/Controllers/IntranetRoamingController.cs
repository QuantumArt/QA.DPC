﻿using System;
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
    [Route("api/vsr")]
    public class InRoamingController : BaseController
    {

        private readonly IntranetRoamingCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        protected JObject Scale;


        public InRoamingController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory)
        {
            _calc = new IntranetRoamingCalculator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, [FromQuery] string region, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            var result = await LoadProducts(id, serviceIds, searchOptions);
            result = result ?? await CorrectProductWithScale(id, region, searchOptions);
            result = result ?? CalculateImpact();
            result = result ?? Content(Product.ToString());
            return result;


        }

        private async Task<ActionResult> CorrectProductWithScale(int id, string region, SearchOptions searchOptions)
        {
            var result = await LoadScale(id, region, searchOptions);
            if (result == null)
            {
                try
                {
                    Product["Parameters"] = _calc.GetResultParameters(Scale, Product, region);
                }
                catch (Exception ex)
                {
                    result = BadRequest($"Exception occurs while merging scale and tariff: {ex.Message}");
                }

            }
            return result;
        }

        private async Task<ActionResult> LoadScale(int id, string region, SearchOptions searchOptions)
        {
            var scaleIds =
                Product.SelectTokens("RoamingScalesOnTariff.[?(@.Id)].RoamingScale.Id").Select(n => (int) n).ToArray();

            JObject[] scales;
            try
            {
                scales = await SearchRepo.GetProducts(scaleIds, searchOptions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Exception occurs while getting scales: {ex.Message}");
            }

            Scale = _calc.FilterScale(region, scales);

            return Scale == null ? NotFound($"Roaming scale is not found for tariff {id} in regions: {region}, {ConfigurationOptions.RootRegionId}") : null;
        }

    }
}
