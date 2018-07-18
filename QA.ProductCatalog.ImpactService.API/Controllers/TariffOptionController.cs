﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    [Route("api/base")]
    public class TariffOptionController : BaseController
    {

        private readonly TariffOptionCalculator _calc;

        protected override BaseImpactCalculator Calculator => _calc;

        public TariffOptionController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
            _calc = new TariffOptionCalculator();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id, [FromQuery] int[] serviceIds, string homeRegion, string state = ElasticIndex.DefaultState, string language = ElasticIndex.DefaultLanguage, bool html = false)
        {

            var searchOptions = new SearchOptions()
            {
                BaseAddress = ConfigurationOptions.ElasticBaseAddress,
                IndexName = ConfigurationOptions.GetIndexName(state, language),
                HomeRegion = homeRegion
            };

            var cacheKey = GetCacheKey(GetType().ToString(), id, serviceIds, homeRegion, homeRegion, state, language);
            var disableCache = html || ConfigurationOptions.CachingInterval <= 0;
            var result = (!disableCache) ? await GetCachedResult(cacheKey, searchOptions) : null;
            if (result != null) return result;


            result = await LoadProducts(id, serviceIds, searchOptions);

            if (ConfigurationOptions.LoadDefaultServices && Services != null && !Services.Any())
            {
                result = result ?? await LoadDefaultService(searchOptions);
            }

            result = result ?? FilterServicesOnProduct(true);

            LogStartImpact("Base", id, serviceIds);

            result = result ?? CalculateImpact(homeRegion);

            LogEndImpact("Base", id, serviceIds);

            result = result ?? (html ? TestLayout(Product, serviceIds, state, language, homeRegion) : Content(Product.ToString()));

            if (!disableCache)
            {
                SetCachedResult(id, serviceIds, result, cacheKey);
            }

            return result;
        }

        private async Task<ActionResult> LoadDefaultService(SearchOptions searchOptions)
        {
            ActionResult result = null;

            try
            {

                Logger.LogTrace($"Checking for default service");
                var defaultServiceId = GetDefaultServiceId();
                if (defaultServiceId == 0) return null;

                Logger.LogTrace($"Start loading default service {defaultServiceId}");
                var results = await SearchRepo.GetProducts(new[] { defaultServiceId }, searchOptions);
                Logger.LogTrace($"End loading default service {defaultServiceId}");

                if (results.Length == 0)
                {
                    var message = $"Service {defaultServiceId} is not found";
                    Logger.LogError(message);
                    return NotFound(message);
                }

                Services = new [] { results[0] };
            }
            
            catch (Exception ex)
            {
                var message = $"Exception occurs while using Elastic Search: {ex.Message}";
                Logger.LogError(1, ex, message);
                result = BadRequest(message);
            }
            return result;
        }

        private int GetDefaultServiceId()
        {
            var defaultServiceId = 0;
            var links = Product.SelectTokens("ServicesOnTariff.[?(@.Service)]");

            foreach (var link in links)
            {
                var modifiers =
                    new HashSet<string>(link.SelectTokens($"Parent.Modifiers.[?(@.Alias)].Alias").Select(n => n.ToString()));
                if (!modifiers.Contains(Calculator.LinkModifierName) || !modifiers.Contains("IsDefault")) continue;
                defaultServiceId = (int) link.SelectToken("Service.Id");
                break;
            }
            return defaultServiceId;
        }
    }
}
