using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;
using QA.Core;
using QA.Core.DPC.QP.Models;
using QA.ProductCatalog.WebApi.Controllers;
using System.Web;
using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.WebApi.App_Start
{
	public class UnityResolver : IDependencyResolver
	{
		private readonly IUnityContainer _container;
		private readonly ILogger _logger;

		/// <summary>
		/// взято отсюда http://www.asp.net/web-api/overview/advanced/dependency-injection
		/// </summary>
		/// <param name="container"></param>
		public UnityResolver(IUnityContainer container)
		{
			if (container == null)
				throw new ArgumentNullException("container");

			_container = container;

			_logger = container.Resolve<ILogger>();
		}

		public object GetService(Type serviceType)
		{
			try
			{
                if (serviceType == typeof(ProductController) && HttpContext.Current != null)
                {
                    var route = HttpContext.Current.Request.RequestContext.RouteData;
                    var customerCode = route.Values["customerCode"] as string;
                    _container.Resolve<IIdentityProvider>().Identity = new Identity(customerCode);
                }

                return _container.Resolve(serviceType);
			}
			catch (ResolutionFailedException)
			{
				return null;
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			try
			{
				return _container.ResolveAll(serviceType);
			}
			catch (ResolutionFailedException)
			{
				return new List<object>();
			}
		}

		public IDependencyScope BeginScope()
		{
			var child = _container.CreateChildContainer();

			return new UnityResolver(child);
		}

		public void Dispose()
		{
			_container.Dispose();

			_logger.Dispose();
		}
	}
}
