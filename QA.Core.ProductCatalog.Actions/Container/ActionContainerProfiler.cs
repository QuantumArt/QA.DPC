﻿using Microsoft.Practices.Unity;
using QA.Core.DPC.Loader;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Decorators;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.ProductCatalog.Actions.Container
{
	public class ActionContainerProfiler : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<IArticleService, ArticleServiceProfiler>(new InjectionFactory(c => new ArticleServiceProfiler(c.Resolve<ArticleServiceAdapter>(), c.Resolve<ILogger>())));
			Container.RegisterType<IProductService, ProductServiceProfiler>(new InjectionFactory(c => new ProductServiceProfiler(c.Resolve<ProductLoader>(), c.Resolve<ILogger>())));
			Container.RegisterType<IQPNotificationService, QPNotificationServiceProfiler>(new InjectionFactory(c => new QPNotificationServiceProfiler(c.Resolve<QPNotificationService>(), c.Resolve<ILogger>())));
			Container.RegisterType<IXmlProductService, XmlProductServiceProfiler>(new InjectionFactory(c => new XmlProductServiceProfiler(c.Resolve<XmlProductService>(), c.Resolve<ILogger>())));

			var a = typeof(ActionContainerConfiguration).Assembly;
			foreach (var t in a.GetExportedTypes())
			{
				if (typeof(IAction).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					Container.RegisterType<IAction, ActionProfiler>(t.Name, new InjectionFactory(c => new ActionProfiler((IAction)c.Resolve(t), c.Resolve<ILogger>())));
				}
			}
		}
	}
}
