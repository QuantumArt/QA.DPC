using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
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
            ProductsOptionsRoot options, string language, string state, CancellationToken cancellationToken)
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

                if (options.DataFilters.Any())
                {
                    result = PostProcess(searchResult, options.DataFilters);
                }
                else
                {
                    result = await GetResponse(searchResult);
                }

                if (useCaching)
                {
                    Cache.Set(key, result, GetCacheOptions((int)options.CacheForSeconds));
                }
            }

            return result;
        }

        protected ActionResult PostProcess(string input, Dictionary<string, string> optionsDataFilters)
        {
            const string sourceQuery = "hits.hits.[?(@._source)]._source";
            var hits = JObject.Parse(input).SelectTokens(sourceQuery).ToArray();
            foreach (var df in optionsDataFilters)
            {
                foreach (var hit in hits)
                {
                    var jArrays = hit.SelectTokens(df.Key).OfType<JArray>().ToArray();
                    foreach (var jArray in jArrays)
                    {
                        var relevantTokens = jArray.SelectTokens(df.Value).ToArray();
                        jArray.Clear();
                        foreach (var rToken in relevantTokens)
                        {
                            jArray.Add(rToken);
                        }
                    }
                }
            }

            return Json(new JArray(hits.Select(n => (object)n)));
        }

        protected async Task<ActionResult> GetResponse(string text, bool filter = true)
        {
            var result = text;
            if (filter)
            {
                var sb = new StringBuilder();
                await JsonFragmentExtractor.ExtractJsonFragment("_source", text, sb, 4);
                result = sb.ToString();
            }
            return Content(result, "application/json");
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
