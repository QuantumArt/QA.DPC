using System;
using QA.Core.DPC.API.Search;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.Actions.Services;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Unity.Extension;
#if NETSTANDARD
using Microsoft.AspNetCore.Http;
#else
using QA.Core.Web;
#endif
using Unity.Injection;
using QA.ProductCatalog.Integration.DAL;
using Unity.Lifetime;

namespace QA.Core.DPC.API.Container
{
    public class APIContainerConfiguration : UnityContainerExtension
	{
		public ITypeLifetimeManager GetHttpContextLifeTimeManager()
		{
#if !NETSTANDARD
			return new HttpContextLifetimeManager();
#else
            return new HttpContextCoreLifetimeManager(Container.Resolve<IHttpContextAccessor>());
#endif
		}		
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
			Container.RegisterFactory<ArticleService>(c => c.Resolve<IServiceFactory>().GetArticleService());
			Container.RegisterType<IArticleService, ArticleServiceAdapter>();
			Container.RegisterFactory<FieldService>(c => c.Resolve<IServiceFactory>().GetFieldService());
			Container.RegisterType<IFieldService, FieldServiceAdapter>(GetHttpContextLifeTimeManager());
			Container.RegisterFactory<ITransaction>(c => new Transaction(c.Resolve<IConnectionProvider>(), c.Resolve<ILogger>()));
			Container.RegisterFactory<Func<ITransaction>>(c => new Func<ITransaction>(() => c.Resolve<ITransaction>()));
			Container.RegisterType<IQPNotificationService, QPNotificationService>();
			Container.RegisterType<IXmlProductService, XmlProductService>();
        }
	}
}
