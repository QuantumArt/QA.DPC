using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Filters
{
    public class TmfProductCustomActionAttribute : TypeFilterAttribute
    {
        public TmfProductCustomActionAttribute() : base(typeof(TmfProductCustomActionFilter))
        {

        }

        private class TmfProductCustomActionFilter : IAuthorizationFilter
        {
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!context.HttpContext.Request.Query.TryGetValue("tmfEnabled", out StringValues tmfEnabledValue))
                {
                    return;
                }

                if (!bool.TryParse(tmfEnabledValue.First(), out bool tmfEnabled))
                {
                    return;
                }

                if (!tmfEnabled)
                {
                    return;
                }

                context.HttpContext.Items[InternalTmfSettings.TmfItemIdentifier] = true;
                context.HttpContext.Items[InternalTmfSettings.QueryResolverContextItemName] = true;
            }
        }
    }
}