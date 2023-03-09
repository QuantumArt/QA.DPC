using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.Core.Models;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Filters
{
    public class TmfProductFormatAttribute : TypeFilterAttribute
    {
        public TmfProductFormatAttribute() : base(typeof(TmfProductFormatFilter))
        {
        }

        private class TmfProductFormatFilter : IAuthorizationFilter
        {
            private readonly ISettingsService _settingsService;

            public TmfProductFormatFilter(ISettingsService settingsService)
            {
                _settingsService = settingsService;
            }
            
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled) &&
                    tmfEnabled)
                {
                    context.HttpContext.Items[InternalTmfSettings.TmfItemIdentifier] = true;

                    context.HttpContext.Items[InternalTmfSettings.ArticleFilterContextItemName] =
                        ArticleFilter.DefaultFilter;

                    context.HttpContext.Items[InternalTmfSettings.RegionTagsContextItemName] = false;
                }
                else
                {
                    context.Result = new NotFoundResult();
                }
            }
        }
    }
}
