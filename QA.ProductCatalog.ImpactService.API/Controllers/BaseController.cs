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
    public abstract class BaseController : Controller
    {
        protected readonly ISearchRepository SearchRepo;

        protected readonly ConfigurationOptions ConfigurationOptions;

        protected ILogger Logger { get; }


        protected JObject Product;

        protected JObject[] Services;

        protected BaseController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory)
        {
            SearchRepo = searchRepo;
            ConfigurationOptions = elasticIndexOptionsAccessor.Value;
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected abstract BaseImpactCalculator Calculator { get; }

        protected async Task<ActionResult> LoadProducts(int id, int[] serviceIds, SearchOptions searchOptions)
        {
            ActionResult result = null;
            try
            {
                var allProductIds = new[] { id }.Union(serviceIds).ToArray();
                var results = await SearchRepo.GetProducts(allProductIds, searchOptions);

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
                return BadRequest($"Exception occurs while calculating impact: {ex.Message}");
            }
            return null;
        }
    }
}
