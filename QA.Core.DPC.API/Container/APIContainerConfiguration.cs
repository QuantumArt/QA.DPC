using System;
using Microsoft.Practices.Unity;
using QA.Core.DPC.API.Search;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog;
using QA.Core.ProductCatalog.Actions.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.Web;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.DPC.API.Container
{
	public class APIContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.AddNewExtension<ActionContainerConfiguration>();

			Container.RegisterType<IProductAPIService, ProductAPIService>();
			Container.RegisterExpressionArticleMatchService();
			Container.RegisterArticleMatchService<ProductQuery, QueryConditionMapper>(c => c.GetConnectionString());
			Container.RegisterType<IProductSearchService, ProductSearchService>();
			Container.RegisterType<IProductUpdateService, ProductUpdateService>();

			Container.RegisterType<IServiceFactory, ServiceFactory>(new InjectionFactory(c => new ServiceFactory(c.GetConnectionString(), c.Resolve<UserProvider>())));
			Container.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
			Container.RegisterType<IArticleService, ArticleServiceAdapter>(new InjectionFactory(c => new ArticleServiceAdapter(c.Resolve<ArticleService>(), c.GetConnectionString(), c.Resolve<IContextStorage>())));
			Container.RegisterType<FieldService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetFieldService()));
			Container.RegisterType<IFieldService, FieldServiceAdapter>(new HttpContextLifetimeManager(), new InjectionFactory(c => new FieldServiceAdapter(c.Resolve<FieldService>(), c.GetConnectionString())));
			Container.RegisterType<ITransaction, Transaction>(new InjectionFactory(c => new Transaction(c.GetConnectionString(), Container.Resolve<ILogger>())));
			Container.RegisterType<Func<ITransaction>>(new InjectionFactory(c => new Func<ITransaction>(() => c.Resolve<ITransaction>())));
			Container.RegisterType<IQPNotificationService, QPNotificationService>();
			Container.RegisterType<IXmlProductService, XmlProductService>();
		}
	}
}
