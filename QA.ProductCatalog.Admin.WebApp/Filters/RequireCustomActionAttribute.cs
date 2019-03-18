using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QA.DPC.Core.Helpers;

namespace QA.ProductCatalog.Admin.WebApp.Filters
{
    public class RequireCustomActionAttribute : TypeFilterAttribute
    {
        public RequireCustomActionAttribute() : base(typeof(RequireCustomActionImpl))
        {
        }

        private class RequireCustomActionImpl : IAuthorizationFilter
        {
            private readonly ISecurityChecker _checker;

            public RequireCustomActionImpl(
                ISecurityChecker checker
            )
            {
                _checker = checker;
            }


            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!_checker.CheckAuthorization())
                {
                    context.Result = new UnauthorizedResult();
                }
            }
        }
    }
}
    