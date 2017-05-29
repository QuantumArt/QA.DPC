using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{
    public class RateLimitAttribute : Attribute, IAsyncActionFilter
    {
        private readonly IOptions<Users> _users;

        private readonly IOptions<RateLimitOptions> _options;

        private readonly ObjectCache _cache;

        public string Profile { get; set; }

        public RateLimitAttribute(IOptions<Users> users, IOptions<RateLimitOptions> options, ObjectCache cache, string profile)
        {
            _users = users;
            _options = options;
            _cache = cache;
            Profile = profile;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var ip = context.HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var actionContext = context.HttpContext;
            var token = actionContext.Request.Headers
                .FirstOrDefault(h => h.Key == "X-Auth-Token")
                .Value.FirstOrDefault();

            var user = _users?.Value
                           ?.FirstOrDefault(u => u.Value == token)
                           .Key ?? "Default";

            var key = GetKey(user, ip, context);

            var counter = _cache[key] as RateCounter;
            var limit = _options.Value[Profile][user].Limit;
            var seconds = _options.Value[Profile][user].Seconds;
            if (counter == null)
            {
                counter = new RateCounter(limit, seconds);
                _cache.Add(key, counter, counter.Reset);
            }

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

    }

    public class RateLimitOptions : Dictionary<string, RateLimitProfile>
    { }
    public class RateLimitProfile : Dictionary<string, RateLimit>
    { }

    public class RateLimit
    {
        public int Seconds { get; set; }
        public int Limit { get; set; }
    }
}