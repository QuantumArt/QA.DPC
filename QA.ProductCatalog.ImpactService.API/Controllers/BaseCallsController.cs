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
    public abstract class BaseCallsController : BaseController
    {

        protected IEnumerable<JToken> Parameters;

        protected IEnumerable<JToken> ServicesOnTariff;

        protected BaseCallsController(ISearchRepository searchRepo, IOptions<ConfigurationOptions> elasticIndexOptionsAccessor, ILoggerFactory loggerFactory) : base(searchRepo, elasticIndexOptionsAccessor, loggerFactory)
        {
        }

        protected abstract BaseCallsImpactCalculator CallsCalculator { get; }

        protected ContentResult GetNewProduct()
        {
            var newProduct = new JObject(new JProperty("Parameters", new JArray(Parameters)),
                new JProperty("ServicesOnTariff", ServicesOnTariff));

            CallsCalculator.Reorder(newProduct);

            var contentResult = Content(newProduct.ToString());
            return contentResult;
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

        protected ActionResult FilterServicesOnTariff()
        {
            try
            {
                ServicesOnTariff = CallsCalculator.FilterServicesOnTariff((JArray) Product.SelectToken("ServicesOnTariff"));
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
