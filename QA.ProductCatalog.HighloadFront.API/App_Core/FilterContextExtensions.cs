using System.Net.Http;
using System.Web.Http.Controllers;
using Autofac;
using Autofac.Integration.WebApi;

namespace QA.ProductCatalog.HighloadFront.App_Core
{
    public static class FilterContextExtensions
    {
        public static T Resolve<T>(this HttpActionContext context)
        {
            var requestScope = context.Request.GetDependencyScope();
            var lifeTimeScope = requestScope.GetRequestLifetimeScope();
            return lifeTimeScope.Resolve<T>();
        }
    }
}