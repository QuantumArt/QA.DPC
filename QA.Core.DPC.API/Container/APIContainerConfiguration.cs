using System;
using QA.Core.DPC.API.Search;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.Web;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Unity.Extension;
using Unity.Injection;
using QA.ProductCatalog.Integration.DAL;

namespace QA.Core.DPC.API.Container
{
    public class APIContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.AddNewExtension<ActionContainerConfiguration>();

			Container.RegisterType<IProductAPIService, ProductAPIService>();
			Container.RegisterExpressionArticleMatchService();
			Container.RegisterArticleMatchService<ProductQuery, QueryConditionMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
            Container.RegisterArticleMatchService<ExtendedProductQuery, ExtendedQueryConditionMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
            Container.RegisterType<IProductSearchService, ProductSearchService>();
			Container.RegisterType<IProductUpdateService, ProductUpdateService>();
            Container.RegisterType<IProductRelevanceService, ProductRelevanceService>();            

            Container.RegisterType<IServiceFactory, ServiceFactory>();
			Container.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
			Container.RegisterType<IArticleService, ArticleServiceAdapter>();
			Container.RegisterType<FieldService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetFieldService()));
			Container.RegisterType<IFieldService, FieldServiceAdapter>(new HttpContextLifetimeManager());
			Container.RegisterType<ITransaction>(new InjectionFactory(c => new Transaction(c.Resolve<IConnectionProvider>(), c.Resolve<ILogger>())));
			Container.RegisterType<Func<ITransaction>>(new InjectionFactory(c => new Func<ITransaction>(() => c.Resolve<ITransaction>())));
			Container.RegisterType<IQPNotificationService, QPNotificationService>();
			Container.RegisterType<IXmlProductService, XmlProductService>();
        }
	}
}
