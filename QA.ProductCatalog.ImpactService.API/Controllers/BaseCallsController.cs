using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.ImpactService.API.Services;

namespace QA.ProductCatalog.ImpactService.API.Controllers
{
    public abstract class BaseCallsController : BaseController
    {

        protected IEnumerable<JToken> Parameters;

        protected BaseCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory, IMemoryCache cache) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory, cache)
        {
        }

        protected abstract BaseCallsImpactCalculator CallsCalculator { get; }

        protected JObject GetNewProduct(string homeRegion)
        {
            var forcedOverride = CallsCalculator.GetGroupOrderOverride(Product, homeRegion);

            var newProduct = new JObject(new JProperty("Parameters", new JArray(Parameters)),
                new JProperty("ServicesOnTariff", ServicesOnProduct));

            CallsCalculator.Reorder(newProduct, homeRegion, forcedOverride);

            return newProduct;
        }

        protected ActionResult FilterServiceParameters(string region)
        {
            
            try
            {
                CallsCalculator.FilterServicesParameters(Services, region);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while filtering parameters: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }

        protected ActionResult FilterProductParameters(string region)
        {
            try
            {
                Parameters = CallsCalculator.FilterProductParameters((JArray) Product.SelectToken("Parameters"), region);
            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while filtering parameters: {ex.Message}";
                Logger.LogError(1, ex, message);
                return BadRequest(message);
            }
            return null;
        }
    }
}
