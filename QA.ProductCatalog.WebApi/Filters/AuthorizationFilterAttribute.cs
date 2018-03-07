using QA.Core.DPC.QP.Services;
using System.Web.Http;
using System.Web.Http.Controllers;

#pragma warning disable 1591
namespace QA.ProductCatalog.WebApi.Filters
{
    public class AuthorizationFilterAttribute : AuthorizeAttribute
    {
        private readonly IIdentityProvider _identityProvider;

        public AuthorizationFilterAttribute(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!_identityProvider.Identity.IsAuthenticated)
            {
                HandleUnauthorizedRequest(actionContext);
            }
        }
    }
}