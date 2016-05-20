using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QA.ProductCatalog.HighloadFront.Filters
{
    public enum ResponseCacheLocation
    {

        /// Sets "Cache-control" header to "public".
        Any = 0,

        /// Sets "Cache-control" header to "private".
        Client = 1,

        /// "Cache-control" and "Pragma" headers are set to "no-cache".
        None = 2
    }

    public class ResponseCacheAttribute : Attribute, IActionFilter
    {
        public int Duration { get; set; }

        public ResponseCacheLocation Location { get; set; } = ResponseCacheLocation.Any;

        public bool NoStore { get; set; }

        public string VaryByHeader { get; set; }

        public void SetupCache(HttpResponseMessage context)
        {
            var headers = context.Headers;

            headers.Remove("Vary");
            headers.Remove("Cache-Control");
            headers.Remove("Pragma");

            if (!string.IsNullOrEmpty(VaryByHeader))
            {
                headers.Add("Vary", VaryByHeader);
            }

            if (NoStore)
            {;
                var cacheControl = Location != ResponseCacheLocation.None ? "no-store" : "no-store, no-cache";
                headers.Add("Cache-Control", cacheControl);
                headers.Add("Pragma", "no-cache");
            }
            else
            {
                string cacheControlValue = null;
                switch (Location)
                {
                    case ResponseCacheLocation.Any:
                        cacheControlValue = "public";
                        break;
                    case ResponseCacheLocation.Client:
                        cacheControlValue = "private";
                        break;
                    case ResponseCacheLocation.None:
                        cacheControlValue = "no-cache";
                        headers.Add("Pragma", "no-cache");
                        break;
                }

                cacheControlValue = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}{1}max-age={2}",
                    cacheControlValue,
                    cacheControlValue != null ? "," : null,
                    Duration);

                headers.Add("Cache-Control", cacheControlValue);
            }
        }

        public bool AllowMultiple { get; } = false;
        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var result = await continuation();
            SetupCache(result);
            return result;
        }
    }
}