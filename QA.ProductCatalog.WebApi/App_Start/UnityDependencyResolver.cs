﻿using System;
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
using QA.Core.Logger;

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
		    _container = container ?? throw new ArgumentNullException(nameof(container));
			_logger = container.Resolve<ILogger>();
		}

		public object GetService(Type serviceType)
		{
			try
			{
                if (serviceType == typeof(ProductController) && HttpContext.Current != null)
                {
                    _container.Resolve<IdentityResolver>().ResolveIdentity(HttpContext.Current.Request);
                }

                return _container.Resolve(serviceType);
			}
			catch (ResolutionFailedException ex)
			{
			    if (serviceType == typeof(ProductController))
			    {
			        _logger.Error(ex.Message);
                }
			    else
			    {
			        _logger.Debug(ex.Message);
			    }
                return null;
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			try
			{
				return _container.ResolveAll(serviceType);
			}
			catch (ResolutionFailedException ex)
			{
			    _logger.Error(ex.Message);
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
