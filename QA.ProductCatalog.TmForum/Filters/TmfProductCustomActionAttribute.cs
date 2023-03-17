using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using QA.ProductCatalog.ContentProviders;
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
            private readonly IOptions<TmfSettings> _tmfSettings;
            private readonly ISettingsService _settingsService;

            public TmfProductCustomActionFilter(IOptions<TmfSettings> tmfSettings, ISettingsService settingsService)
            {
                _tmfSettings = tmfSettings;
                _settingsService = settingsService;
            }
            
            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!_tmfSettings.Value.IsEnabled)
                {
                    return;
                }

                if (!bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool isTmfEnabled) ||
                    !isTmfEnabled)
                {
                    return;
                }
                
                context.HttpContext.Items[InternalTmfSettings.QueryResolverContextItemName] = true;
            }
        }
    }
}