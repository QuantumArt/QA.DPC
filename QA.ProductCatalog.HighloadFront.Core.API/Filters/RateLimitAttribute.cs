using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.Core.Cache;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {

        private readonly IElasticConfiguration _configuration;
        private readonly IVersionedCacheProvider2 _cacheProvider;

        public string Profile { get; set; }

        public RateLimitAttribute(IElasticConfiguration configuration, IVersionedCacheProvider2 cacheProvider,
            string profile)
        {
            _configuration = configuration;
            _cacheProvider = cacheProvider;
            Profile = profile;
        }

        public virtual string GetActualProfile(ActionExecutingContext context)
        {
            return Profile;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ip = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var actionContext = context.HttpContext;
            var token = actionContext.Request.Headers
                .FirstOrDefault(h => h.Key == "X-Auth-Token")
                .Value.FirstOrDefault();

            var user = _configuration.GetUserName(token) ?? "Default";
            var key = GetKey(user, ip, context);
            var limit = _configuration.GetLimit(user, GetActualProfile(context));
            var counter = _cacheProvider.GetOrAdd(key, TimeSpan.FromSeconds(limit.Seconds),
                () => new RateCounter(limit.Limit, limit.Seconds));

            if (counter.Remaining == 0)
            {
                context.Result = new ContentResult()
                {
                    Content = $@"{{""message"":""API rate limit exceeded for {ip}"",""documentation_url"":""docurl""}}",
                    ContentType = "application/json",
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            else
            {
                await next();

                counter.Hit();

                context.HttpContext.Response.Headers.Add("X-RateLimit-Limit", counter.Limit.ToString());
                context.HttpContext.Response.Headers.Add("X-RateLimit-Remaining", counter.Remaining.ToString());
                context.HttpContext.Response.Headers.Add("X-RateLimit-Reset", counter.ResetUnix.ToString());

            }
        }

        private static string GetKey(string user, string ip, ActionExecutingContext actionContext)
        {
            var actionName = (actionContext.ActionDescriptor as ControllerActionDescriptor)?.ActionName;
            var controllerName = (actionContext.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;
            return $"{controllerName}-{actionName}-{user}-{ip}";
        }

        private class RateCounter
        {
            public int Remaining { get; private set; }
            public int Limit { get; }
            public long ResetUnix { get; }
            private DateTime Reset { get; }

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

    }

}