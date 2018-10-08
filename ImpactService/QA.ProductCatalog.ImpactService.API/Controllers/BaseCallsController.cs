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

        protected JObject GetNewProduct(JObject homeRegion)
        {

            var newProduct = new JObject(new JProperty("Parameters", new JArray(Parameters)),
                new JProperty("ServicesOnTariff", ServicesOnProduct));

            CallsCalculator.Reorder(newProduct, homeRegion);

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
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }

        protected ActionResult FilterProductParameters(string region, bool generateNewTitles = true, bool reassignParameters = true)
        {
            try
            {
                var parameters = CallsCalculator.FilterProductParameters((JArray) Product.SelectToken("Parameters"),
                    region, generateNewTitles);
                if (reassignParameters)
                {
                    Parameters = parameters;
                }

            }
            catch (Exception ex)
            {
                var message = $"Exception occurs while filtering parameters: {ex.Message}";
                LogException(ex, message, null);
                return BadRequest(message);
            }
            return null;
        }
    }
}
