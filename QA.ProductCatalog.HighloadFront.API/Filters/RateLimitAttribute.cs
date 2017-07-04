using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Filters
{
    public class RateLimitAttribute: Attribute, IActionFilter
    {
        public bool AllowMultiple { get; } = false;
        public string Profile { get; set; }

        public RateLimitAttribute(string profile)
        {
            Profile = profile;
        }


        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var users = actionContext.Resolve<IOptions<Users>>();
            var options = actionContext.Resolve<IOptions<RateLimitOptions>>();
            var cache = actionContext.Resolve<ObjectCache>();

            var ip = GetClientIp(actionContext.Request);

            var tocken = actionContext.Request.Headers
                .FirstOrDefault(h=>h.Key == "X-Auth-Token")
                .Value?.FirstOrDefault();

            var user = users?.Value
                ?.FirstOrDefault(u => u.Value == tocken)
                .Key ?? "Default";

            var key = GetKey(user, ip, actionContext);

            var counter = cache[key] as RateCounter;
            var limit = options.Value[Profile][user].Limit;
            var seconds = options.Value[Profile][user].Seconds;
            if (counter == null)
            {
                counter = new RateCounter(limit, seconds);
                cache.Add(key, counter, counter.Reset);
            }
            HttpResponseMessage result;
            if (counter.Remaining == 0)
            {
                //todo вынести в ресурсы
                var response =
                    $@"{{""message"":""API rate limit exceeded for {ip}"",""documentation_url"":""docurl""}}";
                result = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent(response, Encoding.Default, "application/json")
                };
            }
            else
            {
                result = await continuation();
            }

            counter.Hit();
            result.Headers.Add("X-RateLimit-Limit", counter.Limit.ToString());
            result.Headers.Add("X-RateLimit-Remaining", counter.Remaining.ToString());
            result.Headers.Add("X-RateLimit-Reset", counter.ResetUnix.ToString());
            return result;
        }

        private static string GetKey(string user, string ip, HttpActionContext actionContext)
        {
            var actionName = actionContext.ActionDescriptor.ActionName;
            var controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            return $"{controllerName}-{actionName}-{user}-{ip}";
        }

        private class RateCounter
        {
            public int Remaining { get; private set; }
            public int Limit { get; }
            public long ResetUnix { get; }
            public DateTime Reset { get; }

            public void Hit()
            {
                if (Remaining > 0) Remaining--;
            }

            public RateCounter(int limit, int seconds)
            {
                Remaining = Limit = limit;
                Reset = DateTime.Now.AddSeconds(seconds);
                ResetUnix = Reset.ToUnixTimestamp();
            }
        }

        private static string GetClientIp(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }

            return !request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name) ? null 
                : ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;
        }
    }

    public class RateLimitOptions : Dictionary<string, RateLimitProfile>
    {}
    public class RateLimitProfile: Dictionary<string, RateLimit>
    {}

    public class RateLimit
    {
        public int Seconds { get; set; }
        public int Limit { get; set; }
    }
}