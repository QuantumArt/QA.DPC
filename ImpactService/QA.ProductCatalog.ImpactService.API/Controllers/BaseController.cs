using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Fluent;
using QA.ProductCatalog.ImpactService.API.Helpers;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ISearchRepository SearchRepo;

        protected readonly ConfigurationOptions ConfigurationOptions;

        protected readonly IMemoryCache Cache;

        protected abstract Logger Logger { get; }

        protected JObject Product;

        protected JObject[] Services;

        protected int[] ServiceIds => Services.Select(n => (int) n.SelectToken("Id")).ToArray();

        protected JObject[] ParameterGroups;

        protected IEnumerable<JToken> ServicesOnProduct;

        protected BaseController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, IMemoryCache cache)
        {
            SearchRepo = searchRepo;
            ConfigurationOptions = elasticIndexOptionsAccessor.Value;
            Cache = cache;
        }

        protected abstract BaseImpactCalculator Calculator { get; }


        protected async Task<DateTimeOffset> GetLastUpdated(int[] ids, SearchOptions searchOptions, DateTimeOffset defaultValue)
        {
            var addrString = GetAddressString(searchOptions);
            var productIds = string.Join(", ", ids);
            Log(LogLevel.Trace, "Check last updated for: {productIds}", searchOptions, productIds);
            try
            {
                return await SearchRepo.GetLastUpdated(ids, searchOptions, defaultValue);
            }
            catch (Exception e)
            {
                var message = $"Exception occurs while using Elastic Search: {e.Message}";
                LogException(e, message, searchOptions);
                return defaultValue;
            }

        }

        protected async Task<bool> IsOneMacroRegion(string region, string homeRegion, SearchOptions options)
        {
            if (string.IsNullOrEmpty(region) || string.IsNullOrEmpty(homeRegion)) return false;
            var addrString = GetAddressString(options);
            Log(LogLevel.Trace, "Check for common macroregion for region {region} and home region {homeRegion}", options, region, homeRegion);
            try
            {
                return await SearchRepo.IsOneMacroRegion(new[] { region, homeRegion }, options);
            }
            catch (Exception e)
            {
                var message = $"Exception occurs while using Elastic Search: {e.Message}";
                LogException(e, message, options);
                return false;
            }
        }

        protected async Task<ActionResult> LoadProducts(int id, int[] serviceIds, SearchOptions searchOptions, bool loadServicesSilently = false)
        {
            ActionResult result = null;
            try
            {
                var message = "Start loading product {id} with services {serviceIds}";
                Log(LogLevel.Trace, message, searchOptions, id, serviceIds);

                var allProductIds = new[] { id }.Union(serviceIds).ToArray();
                var results = await SearchRepo.GetProducts(allProductIds, searchOptions);
                message = "End loading product {id} with services {serviceIds}";
                Log(LogLevel.Trace, message, searchOptions, id, serviceIds);

                Product = results.FirstOrDefault(n => (int) n["Id"] == id);
                if (Product == null)
                {
                    message = "Product {0} is not found";
                    Log(LogLevel.Error, message, searchOptions, id);
                    result = NotFound(String.Format(message, id));
                }
                else
                {
                    var servicesList = new List<JObject>();
                    foreach (var serviceId in serviceIds)
                    {
                        var service = results.FirstOrDefault(m => (int)m["Id"] == serviceId);
                        if (service == null)
                        {
                            if (loadServicesSilently) continue;
                            message = "Service {0} is not found";
                            Log(LogLevel.Error, message, searchOptions, serviceId);
                            result = NotFound(String.Format(message, serviceId));
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
                LogException(ex, message, searchOptions);
                result = BadRequest(message);
            }
            return result;
        }

        private static string GetAddressString(SearchOptions searchOptions)
        {
            return "Address: {address}, Index: {index}";
        }

        protected virtual ActionResult CalculateImpact(JObject homeRegionData)
        {
            try
            {
                Calculator.Calculate(Product, Services.ToArray(), homeRegionData);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while calculating impact: {ex.Message}";
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }

        protected ActionResult TestLayout(JObject product, int[] serviceIds, string state, string language, string homeRegion = null, string region = null, string country = null, string tariffId = null)
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
                Country = country,
                TariffId = tariffId
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

        protected async Task<ActionResult> GetCachedResult(string cacheKey, SearchOptions searchOptions, bool html)
        {
            if (IsCacheDisabled(html)) return null;
            
            Log(LogLevel.Trace, "Cache key: {key}", searchOptions, cacheKey);

            CacheEntry cacheEntry;
            if (!Cache.TryGetValue(cacheKey, out cacheEntry)) return null;
            
            Log(LogLevel.Trace, "Cache hit", searchOptions);

            var storageDate = await GetLastUpdated(cacheEntry.Ids, searchOptions, cacheEntry.LastModified);
            Log(LogLevel.Trace, "LastUpdated with: {date}", searchOptions, storageDate);

            var invalidate = DateTimeOffset.Compare(storageDate, cacheEntry.LastModified) > 0;
            if (invalidate)
            {
                Log(LogLevel.Trace, "Invalidated for: {date}", searchOptions, cacheEntry.LastModified);
                return null;
            }
            else
            {
                return cacheEntry.Product;
            }
        }

        protected void LogEndImpact(string code, int id, int[] serviceIds)
        {
            Log(LogLevel.Trace, "End calculating {code} impact for product {id} and services {serviceIds}", null, code, id, serviceIds);
        }
 
        protected void LogStartImpact(string code, int id, int[] serviceIds)
        {
            Log(LogLevel.Trace, "Start calculating {code} impact for product {id} and services {serviceIds}", null, code, id, serviceIds);
        }

        protected ActionResult FilterServicesOnProduct(bool saveInProduct = false, IEnumerable<int> excludeIds = null, JObject homeRegionData = null)
        {
            try
            {
                ServicesOnProduct = Calculator.FilterServicesOnProduct(Product, excludeIds, homeRegionData).ToArray();
                if (saveInProduct)
                {
                    Calculator.SaveServicesOnProduct(Product, ServicesOnProduct);
                }
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while filtering services: {ex.Message}";
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }

        protected async Task<ActionResult> FillHomeRegion(SearchOptions searchOptions)
        {
            ActionResult result = null;
            try
            {
                if (!string.IsNullOrEmpty(searchOptions.HomeRegion))
                {
                    searchOptions.HomeRegionData = await SearchRepo.GetHomeRegion(searchOptions);
                }
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while loading home region: {ex.Message}";
                LogException(ex, message, searchOptions);
                result = BadRequest(message);
            }
            return result;
        }

                
        protected async Task<string> GetDefaultRegionAliasForMnr(SearchOptions searchOptions)
        {
            var result = "";
            try
            {
                result = await SearchRepo.GetDefaultRegionAliasForMnr(searchOptions);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while loading default region for MNR: {ex.Message}";
                LogException(ex, message, searchOptions);
            }
            return result;
        }
        
        protected async Task<ActionResult> FillDefaultHomeRegion(SearchOptions searchOptions, JObject product)
        {
            ActionResult result = null;
            try
            {
                if (string.IsNullOrEmpty(searchOptions.HomeRegion))
                {
                    var alias = product.SelectTokens("Regions.[?(@.Alias)].Alias").FirstOrDefault()?.ToString();
                    if (!string.IsNullOrEmpty(alias))
                    {
                        searchOptions.HomeRegion = alias;
                        searchOptions.HomeRegionData = await SearchRepo.GetHomeRegion(searchOptions);
                    }                    
                }
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while loading default home region: {ex.Message}";
                LogException(ex, message, searchOptions);
                result = BadRequest(message);
            }
            return result;
        }

        protected void ConfigureOptions(SearchOptions options)
        {
            var key = $"productstore_{options.IndexName}";
            
            options.IndexIsTyped = Cache.GetOrCreate(key, c =>
            {
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(ConfigurationOptions.CachingInterval));
                c.SetOptions(cacheOptions);
                return SearchRepo.GetIndexIsTyped(options).Result;
            });
        }


        private LogBuilder GetLogBuilder(LogLevel level, string message, SearchOptions searchOptions, object[] args)
        {
            var builder = Logger.Log(level).Message(message, args);
            if (searchOptions != null)
            {
                builder.Property("searchOptions", searchOptions);
            }
            return builder;
        }
        
        protected void Log(LogLevel level, string message, SearchOptions searchOptions, params object[] args)
        {
            if (Logger.IsEnabled(level))
            {
                var builder = GetLogBuilder(level, message, searchOptions, args);
                builder.Write();            
            }
        }
        
        protected void LogException(Exception ex, string message, SearchOptions searchOptions, params object[] args)
        {
            var builder = GetLogBuilder(LogLevel.Error, message, searchOptions, args).Exception(ex);
            builder.Write();
        }

        protected bool IsCacheDisabled(bool html = false)
        {
            var result = html || ConfigurationOptions.CachingInterval <= 0;
            if (result && Logger.IsTraceEnabled)
            {
                Logger.Log(LogLevel.Trace,
                    html
                        ? "Cache is disabled because of html mode"
                        : "Cache is disabled because of CachingInterval configuration option");
            }
            return result;
        }
    }
}
