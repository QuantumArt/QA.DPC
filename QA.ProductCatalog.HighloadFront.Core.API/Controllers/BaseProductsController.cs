using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NLog;
using NLog.Fluent;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    public abstract class BaseProductsController : BaseController
    {
        protected readonly SonicElasticStoreOptions ElasticOptions;
        protected readonly IMemoryCache Cache;

        protected BaseProductsController(
            ProductManager manager,
            ElasticConfiguration configuration,
            SonicElasticStoreOptions elasticOptions,
            IMemoryCache cache)
            : base(manager, configuration)
        {
            ElasticOptions = elasticOptions;
            Cache = cache;
        }

        protected async Task<ActionResult> GetSearchActionResult(
            ProductsOptionsBase options, string language, string state, CancellationToken cancellationToken)
        {
            string searchResult = null;
            var key = options.GetKey();
            var useCaching = options.CacheForSeconds > 0 && key != null;
            if (useCaching && Cache.TryGetValue(key, out var cacheResult))
            {
                searchResult = (string) cacheResult;
            }
            else
            {
                searchResult = await Manager.SearchAsync(options, language, state, cancellationToken);

                if (useCaching)
                {
                    var seconds = (int) options.CacheForSeconds;
                    Cache.Set(key, searchResult, GetCacheOptions(seconds));
                    if (TraceApiCalls)
                    {
                        Logger.ForTraceEvent().Message($"Call to ElasticSearch cached for {seconds} seconds")
                            .Property("key", key)
                            .Property("result", searchResult)
                            .Property("language", language)
                            .Property("state", state)
                            .Property("searchOptions", options)
                            .Property("user", HttpContext.GetAuthTokenUser())
                            .Log();
                    }
                }
            }

            return Content(searchResult, "application/json");
        }

        protected BadRequestObjectResult ElasticBadRequest(ElasticClientException ex, int id = 0)
        {
            LogException(ex, "Elastic Search error occurred: ");
            return BadRequest($"Elastic search error occurred: Reason: {ex.Message}");
        }

        protected MemoryCacheEntryOptions GetCacheOptions(int value)
        {
            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromSeconds(value));
            return options;
        }
    }
}
