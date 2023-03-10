using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Filters;

public class DisableDefaultApiAttribute : TypeFilterAttribute
{
    public DisableDefaultApiAttribute() : base(typeof(DisableDefaultApiFilter))
    {

    }
    
    private class DisableDefaultApiFilter : IAuthorizationFilter
    {
        private readonly ISettingsService _settingsService;
        private readonly TmfSettings _tmfSettings;

        public DisableDefaultApiFilter(ISettingsService settingsService, IOptions<TmfSettings> tmfSettings)
        {
            _settingsService = settingsService;
            _tmfSettings = tmfSettings.Value;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled) ||
                !tmfEnabled)
            {
                return;
            }

            if (_tmfSettings.IsEnabled)
            {
                context.Result =
                    new ObjectResult("TmForum support enabled for your customer code. Disable it or use TmForum API instead.")
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };

                return;
            }

            context.Result =
                new ObjectResult("TmForum support enabled for your customer code but disabled in API configuration.")
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
        }
    }
}
