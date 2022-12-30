using Microsoft.AspNetCore.Mvc.Filters;
using QA.Core.DPC.Loader.Container;
using System;

namespace QA.ProductCatalog.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TmfProductFormatAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            context.HttpContext.Items[LoaderConfigurationExtension.TmfItemIdentifier] = true;
        }
    }
}