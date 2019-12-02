using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using Quantumart.QP8.Security;

namespace QA.ProductCatalog.Admin.WebApp.Filters
{
    public class RequireCustomActionAttribute : TypeFilterAttribute
    {
        public RequireCustomActionAttribute() : base(typeof(RequireCustomActionImpl))
        {
        }

        private class RequireCustomActionImpl : IAuthorizationFilter
        {
            private readonly IUserProvider _provider;

            public RequireCustomActionImpl(
                IUserProvider provider
            )
            {
                _provider = provider;
            }


            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (_provider.GetUserId() <= 0)
                {
                    context.Result = new UnauthorizedResult();
                }

                var langId = _provider.GetLanguageId();
                var ci = new CultureInfo(QpUser.GetCultureNameByLanguageId(langId));
                CultureInfo.CurrentCulture = ci;
                CultureInfo.CurrentUICulture = ci;
            }
        }
    }
}
    