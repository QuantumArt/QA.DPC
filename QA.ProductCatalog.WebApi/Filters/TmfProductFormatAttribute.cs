using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.Core.DPC.Loader.Container;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class TmfProductFormatAttribute : TypeFilterAttribute
    {
        public TmfProductFormatAttribute() : base(typeof(TmfProductFormatFilter))
        {
        }

        private class TmfProductFormatFilter : IAuthorizationFilter
        {
            private readonly bool _isTmfEnabled;

            public TmfProductFormatFilter(IOptions<TmfSettings> tmfSettingsOptions)
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
}