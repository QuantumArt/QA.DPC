using System.Web.Http;
using Unity;
using QA.ProductCatalog.FileSyncWebHost.Filters;

namespace QA.ProductCatalog.FileSyncWebHost
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			var container = UnityConfig.Configure();
			config.DependencyResolver = new UnityResolver(container);

			config.Filters.Add(container.Resolve<ExceptionLoggerFilterAttribute>());

			config.Routes.MapHttpRoute(
				name: "Send",
				routeTemplate: "Product/Send/{format}/{exstension}",
				defaults: new { controller = "Product", action = "Send" }
			);

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
