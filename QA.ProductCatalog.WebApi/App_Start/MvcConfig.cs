using System.Web.Mvc;
using System.Web.Routing;
using QA.Core.Logger;
using QA.Core.Web;
using Unity;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public static class MvcConfig
    {
        public static void Init(IUnityContainer container)
        {
            AreaRegistration.RegisterAllAreas();
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            RouteTable.Routes.MapRoute(
                name: "1",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );

            DependencyResolver.SetResolver(new UnityDependencyResolver(container, new NullLogger()));
        }
    }
}