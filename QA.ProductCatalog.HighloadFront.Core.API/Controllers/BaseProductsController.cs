using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
            bool readData = true;
            ActionResult result = null;
            var key = options.GetKey();
            var useCaching = options.CacheForSeconds > 0 && key != null;
            if (useCaching && Cache.TryGetValue(key, out var value))
            {
                result = (ActionResult)value;
                readData = false;
            }

            if (readData)
            {
                var searchResult = await Manager.SearchAsync(options, language, state, cancellationToken);

                result = Json(searchResult);

                if (useCaching)
                {
                    Cache.Set(key, result, GetCacheOptions((int)options.CacheForSeconds));
                }
            }

            return result;
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
