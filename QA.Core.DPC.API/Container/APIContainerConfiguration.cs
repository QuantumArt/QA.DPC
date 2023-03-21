using System;
using QA.Core.DPC.API.Search;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Unity.Extension;
using QA.DPC.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Unity.Injection;
using QA.ProductCatalog.Integration.DAL;
using Unity.Lifetime;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;

namespace QA.Core.DPC.API.Container
{
    public class APIContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.AddNewExtension<ActionContainerConfiguration>();

            Container.RegisterType<IProductAPIService, ProductAPIService>();
            Container.RegisterFactory<Func<IProductAPIService>>(c => new Func<IProductAPIService>(() => c.Resolve<IProductAPIService>()));
            Container.RegisterExpressionArticleMatchService();
            Container.RegisterArticleMatchService<ConditionBase, ExactMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
            Container.RegisterArticleMatchService<ProductQuery, QueryConditionMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
            Container.RegisterArticleMatchService<ExtendedProductQuery, ExtendedQueryConditionMapper>(c => c.Resolve<IConnectionProvider>().GetConnection());
            Container.RegisterType<IProductSearchService, ProductSearchService>();
            Container.RegisterType<IProductUpdateService, ProductUpdateService>();
            Container.RegisterType<IProductRelevanceService, ProductRelevanceService>();            

            Container.RegisterType<IQPNotificationService, QPNotificationService>();
            Container.RegisterType<IXmlProductService, XmlProductService>();
        }
    }
}
