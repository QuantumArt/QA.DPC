using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.Core.DPC.Loader.Container;
using System;

namespace QA.ProductCatalog.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TmfProductFormatAttribute : Attribute, IAuthorizationFilter
    {
        private readonly bool _isTmfEnabled;

        public TmfProductFormatAttribute(IOptions<TMForumSettings> tmfSettingsOptions)
        {
            _isTmfEnabled = tmfSettingsOptions.Value?.IsEnabled ?? false;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (_isTmfEnabled)
            {
                context.HttpContext.Items[LoaderConfigurationExtension.TmfItemIdentifier] = true;
            }
            else
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}