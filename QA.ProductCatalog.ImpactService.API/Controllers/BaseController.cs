using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ISearchRepository SearchRepo;

        protected readonly ConfigurationOptions ConfigurationOptions;

        protected readonly IMemoryCache Cache;

        protected ILogger Logger { get; }


        protected JObject Product;

        protected JObject[] Services;

        protected JObject[] ParameterGroups;
        protected IEnumerable<JToken> ServicesOnProduct;

        protected BaseController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache)
        {
            SearchRepo = searchRepo;
            ConfigurationOptions = elasticIndexOptionsAccessor.Value;
            Logger = loggerFactory.CreateLogger(GetType());
            Cache = cache;
        }

        protected abstract BaseImpactCalculator Calculator { get; }


        protected async Task<DateTimeOffset> GetLastUpdated(int[] ids, SearchOptions searchOptions, DateTimeOffset defaultValue)
        {
            return await SearchRepo.GetLastUpdated(ids, searchOptions, defaultValue);
        }

        protected async Task<ActionResult> LoadProducts(int id, int[] serviceIds, SearchOptions searchOptions)
        {
            ActionResult result = null;
            try
            {
                var services = string.Join(", ", serviceIds);
                Logger.LogTrace($"Start loading product {id} with services {services}");

                var allProductIds = new[] { id }.Union(serviceIds).ToArray();
                var results = await SearchRepo.GetProducts(allProductIds, searchOptions);

                Logger.LogTrace($"End loading product {id} with services {services}");

                Product = results.FirstOrDefault(n => (int) n["Id"] == id);
                if (Product == null)
                {
                    var message = $"Product {id} is not found";
                    Logger.LogError(message);
                    result = NotFound(message);
                }
                else
                {
                    var servicesList = new List<JObject>();
                    foreach (var serviceId in serviceIds)
                    {
                        var service = results.FirstOrDefault(m => (int)m["Id"] == serviceId);
                        if (service == null)
                        {
                            var message = $"Service {serviceId} is not found";
                            Logger.LogError(message);
                            result = NotFound(message);
                        }
                        else
                        {
                            servicesList.Add(service);
                        }
                    }
                    Services = servicesList.ToArray();
                }

            }

            catch (Exception ex)
            {
                var message = $"Exception occurs while using Elastic Search: {ex.Message}";
                Logger.LogError(1, ex, message);
                result = BadRequest(message);
            }
            return result;
        }

        protected virtual ActionResult CalculateImpact()
        {
            try
            {
                Calculator.Calculate(Product, Services.ToArray());
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating impact: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }

        protected ActionResult TestLayout(JObject product, int[] serviceIds, string state, string language, string homeRegion = null, string region = null, string country = null)
        {
            var result = new ProductLayoutModel
            {
                Product = product,
                Calculator = Calculator,
                ServiceIds = serviceIds,
                State = state,
                Language = language,
                Region = region,
                HomeRegion = homeRegion,
                Country = country
            };
            return View("Product", result);
        }

        [Route("integration")]
        // ReSharper disable once InconsistentNaming
        public ActionResult Integration(int content_item_id, string state, string language, bool html = true)
        {
            return RedirectToAction("Get", new {id = content_item_id, html, state, language});
        }

        protected string GetCacheKey(string useCase, int productId, int[] serviceIds, string placeCode,
            string homeRegionCode, string state, string language)
        {
            var sb = new StringBuilder();
            sb.Append($"useCase: {useCase}, ");
            sb.Append($"product: {productId}, ");
            sb.Append($"services: {string.Join(",", serviceIds.OrderBy(m => m))}, ");
            sb.Append($"place: {placeCode}, ");
            sb.Append($"home: {homeRegionCode}, ");
            sb.Append($"state: {state}, ");
            sb.Append($"language: {language}, ");
            return sb.ToString();
        }

        protected void SetCachedResult(int id, int[] serviceIds, ActionResult result, string cacheKey)
        {
            var isNegative = result is NotFoundObjectResult || result is BadRequestObjectResult;

            var newCacheEntry = new CacheEntry()
            {
                Ids = new[] {id}.Union(serviceIds).ToArray(),
                LastModified = DateTimeOffset.Now,
                Product = result
            };

            var memoryCacheEntryOptions = new MemoryCacheEntryOptions();
            if (isNegative)
            {
                memoryCacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromSeconds(ConfigurationOptions.NegativeCachingInterval));
            }
            else
            {
                memoryCacheEntryOptions.SetSlidingExpiration(TimeSpan.FromSeconds(ConfigurationOptions.CachingInterval));
            }

            Cache.Set(cacheKey, newCacheEntry, memoryCacheEntryOptions);
        }

        protected async Task<ActionResult> GetCachedResult(string cacheKey, SearchOptions searchOptions)
        {
            Logger.LogTrace($"Cache key : {cacheKey}");

            CacheEntry cacheEntry;
            if (!Cache.TryGetValue(cacheKey, out cacheEntry)) return null;
            Logger.LogTrace("Cache hit");

            var storageDate = await GetLastUpdated(cacheEntry.Ids, searchOptions, cacheEntry.LastModified);
            Logger.LogTrace($"LastUpdated with: {storageDate}");

            var invalidate = DateTimeOffset.Compare(storageDate, cacheEntry.LastModified) > 0;
            if (invalidate)
            {
                Logger.LogTrace($"Invalidated for: {cacheEntry.LastModified}");
                return null;
            }
            else
            {
                return cacheEntry.Product;
            }
        }

        protected void LogEndImpact(string code, int id, int[] serviceIds)
        {
            var services = string.Join(", ", serviceIds);
            Logger.LogTrace($"End calculating {code} impact for product {id} and services {services}");
        }

        protected void LogStartImpact(string code, int id, int[] serviceIds)
        {
            var services = string.Join(", ", serviceIds);
            Logger.LogTrace($"Start calculating {code} impact for product {id} and services {services}");
        }

        protected ActionResult FilterServicesOnProduct(bool saveInProduct = false)
        {
            try
            {
                ServicesOnProduct = Calculator.FilterServicesOnProduct(Product).ToArray();
                if (saveInProduct)
                {
                    Calculator.SaveServicesOnProduct(Product, ServicesOnProduct);
                }
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while filtering services: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }
    }
}
