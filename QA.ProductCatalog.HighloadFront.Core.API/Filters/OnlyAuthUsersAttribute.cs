using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{

    public class OnlyAuthUsersAttribute : TypeFilterAttribute
    {
        public OnlyAuthUsersAttribute() : base(typeof(OnlyAuthUsersAttributeImpl))
        {
            
        }

        private class OnlyAuthUsersAttributeImpl : Attribute, IAsyncActionFilter
        {
            private readonly IOptions<Users> _users;
            public OnlyAuthUsersAttributeImpl(IOptions<Users> users)
            {
                _users = users;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var token = context.HttpContext.Request.Headers
                    .FirstOrDefault(h => h.Key == "X-Auth-Token")
                    .Value.FirstOrDefault();

                var user = _users?.Value?.FirstOrDefault(u => u.Value == token).Key;

                if (user != null || _users?.Value?.ContainsKey("Default") == true && token == null)
                    await next();
                else
                {
                    context.Result = new ContentResult()
                    {
                        Content = $@"{{""message"":""You have no valid access token""}}",
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
            }
        }
    }

}