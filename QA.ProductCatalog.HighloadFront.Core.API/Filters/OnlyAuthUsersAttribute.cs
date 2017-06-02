using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{

    public class OnlyAuthUsersAttribute : TypeFilterAttribute
    {
        public OnlyAuthUsersAttribute() : base(typeof(OnlyAuthUsersAttributeImpl))
        {
            
        }

        private class OnlyAuthUsersAttributeImpl : Attribute, IAsyncActionFilter
        {
            private readonly IElasticConfiguration _configuration;
            public OnlyAuthUsersAttributeImpl(IElasticConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var token = context.HttpContext.Request.Headers
                    .FirstOrDefault(h => h.Key == "X-Auth-Token")
                    .Value.FirstOrDefault();

                var user = _configuration.GetUserName(token);

                if (user != null || token == null && string.IsNullOrEmpty(_configuration.GetUserToken("Default")))
                    await next();
                else
                {
                    context.Result = new ContentResult()
                    {
                        Content = @"{""message"":""You have no valid access token""}",
                        ContentType = "application/json",
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }
            }
        }
    }

}