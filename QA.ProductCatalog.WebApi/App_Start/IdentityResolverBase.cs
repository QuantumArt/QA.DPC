using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public class IdentityResolverBase
    {
        private const int DefaultUserId = 1;    

        protected readonly IIdentityProvider _identityProvider;

        public IdentityResolverBase(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;         
        }

        public virtual void ResolveIdentity(HttpRequest httpRequest)
        {
            var subroutes = ((IHttpRouteData[])httpRequest.RequestContext.RouteData.Values["MS_SubRoutes"]).FirstOrDefault();
            var customerCode = GetRoute(subroutes, "customerCode");

            _identityProvider.Identity = new Identity(customerCode, GetDefaultUserId(), false);
        }

        protected string GetRoute(IHttpRouteData subroutes, string key)
        {
            if (subroutes.Values.TryGetValue(key, out object value))
            {
                return value as string;
            }
            else
            {
                return null;
            }
        }

        protected int GetDefaultUserId()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["UserId"], out int userId))
            {
                return userId;
            }
            else
            {
                return DefaultUserId;
            }
        }    
    }
}