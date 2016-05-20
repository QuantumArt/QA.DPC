using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Infrastructure;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Filters
{
    public class OnlyAuthUsersAttribute : Attribute, IActionFilter
    {
        public bool AllowMultiple { get; } = false;

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var users = actionContext.Resolve<IOptions<Users>>();

            var token = actionContext.Request.Headers
                .FirstOrDefault(h => h.Key == "X-Auth-Token")
                .Value?.FirstOrDefault();

            var user = users?.Value?.FirstOrDefault(u => u.Value == token).Key;

            if (user != null || users?.Value?.ContainsKey("Default") == true && token == null)
                return await continuation();

            return new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { message = "You have no valid access token" })
                    , Encoding.Default
                    , "application/json")
            };
        }
    }
}