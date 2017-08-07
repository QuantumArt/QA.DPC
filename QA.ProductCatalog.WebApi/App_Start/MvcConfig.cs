using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using QA.Core.Logger;
using QA.Core.Web;

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