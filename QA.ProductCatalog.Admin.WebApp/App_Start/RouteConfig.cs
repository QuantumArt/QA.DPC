using System.Web.Mvc;
using System.Web.Routing;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "1",
                url: "{controller}/publicate/preaction/{id}",
                defaults: new { controller = "Home", action = "PublicatePreAction", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "2",
               url: "{controller}/clone/preaction/{id}",
               defaults: new { controller = "Home", action = "ClonePreAction", id = UrlParameter.Optional }
            );

            routes.MapRoute(
             name: "3",
             url: "{controller}/send/preaction/{id}",
             defaults: new { controller = "Home", action = "SendPreAction", id = UrlParameter.Optional }
            );

			routes.MapRoute(
			  name: "NonInterfaceAction",
			  url: "Action/{command}",
			  defaults: new { controller = "Product", action = "Action" }
			);

			routes.MapRoute(
				 name: "RemoteValidation",
				 url: "RemoteValidation/{validatorKey}",
				 defaults: new { controller = "RemoteValidation", action = "Validate" }
			 );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}