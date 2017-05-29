using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Practices.Unity;
using QA.Core;

namespace QA.ProductCatalog.FileSyncWebHost
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
		    _container = container ?? throw new ArgumentNullException(nameof(container));

			_logger = container.Resolve<ILogger>();
		}

		public object GetService(Type serviceType)
		{
			try
			{
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
