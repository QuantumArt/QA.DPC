using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.TmForum.Filters;

public class DisableDefaultApiAttribute : TypeFilterAttribute
{
    public DisableDefaultApiAttribute() : base(typeof(DisableDefaultApiFilter))
    {

    }
    
    private class DisableDefaultApiFilter : IAuthorizationFilter
    {
        private readonly ISettingsService _settingsService;

        public DisableDefaultApiFilter(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled) &&
                tmfEnabled)
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
