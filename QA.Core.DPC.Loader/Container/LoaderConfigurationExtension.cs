﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using QA.Core.ProductCatalog;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models;
using Quantumart.QP8.BLL;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.Loader.Container
{
    public class LoaderConfigurationExtension : LoaderConfigurationExtensionBase<ProductLoader> { }
    public class LocalSystemCachedLoaderConfigurationExtension : LoaderConfigurationExtensionBase<LocalSystemCachedLoader> { }
    
    public class LoaderConfigurationExtensionBase<TLoader> : UnityContainerExtension
        where TLoader : IProductService
    {
        public const string AlwaysAdminUserProviderName = "AlwaysAdminUserProvider";

        protected override void Initialize()
        {
            Container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));
            Container.RegisterType<IServiceFactory, ServiceFactory>();
            Container.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
            Container.RegisterType<IArticleService, ArticleServiceAdapter>();
            Container.RegisterType<FieldService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetFieldService()));
            Container.RegisterType<IFieldService, FieldServiceAdapter>(new HttpContextLifetimeManager());
            Container.RegisterType<IFreezeService, FreezeService>();

            Container.RegisterType<IFieldService, FieldServiceAdapter>("FieldServiceAdapterAlwaysAdmin",
                new HttpContextLifetimeManager(),
                new InjectionFactory(x => new FieldServiceAdapter(new FieldService(x.Resolve<IConnectionProvider>().GetConnection(), 1), x.Resolve<IConnectionProvider>())));

            Container.RegisterType<ITransaction, Transaction>(new InjectionFactory(c => new Transaction(c.GetConnectionString(), Container.Resolve<ILogger>())));
            Container.RegisterType<Func<ITransaction>>(new InjectionFactory(c => new Func<ITransaction>(() => c.Resolve<ITransaction>())));

            Container.RegisterType<IJsonProductService, JsonProductService>();

            Container.RegisterType<IContextStorage, QpCachedContextStorage>();

            Container.RegisterType<IProductDeserializer, ProductDeserializer>();

            //Фейк юзер нужен для работы ремоут валидации. Также нужны варианты сервисов с фейк-юзером
            Container.RegisterType<IUserProvider, AlwaysAdminUserProvider>(AlwaysAdminUserProviderName);
            Container.RegisterType<IServiceFactory, ServiceFactory>("ServiceFactoryFakeUser", new InjectionFactory(c => new ServiceFactory(c.Resolve<IConnectionProvider>(), c.Resolve<IUserProvider>(AlwaysAdminUserProviderName))));
            Container.RegisterType<ArticleService>("ArticleServiceFakeUser", new InjectionFactory(c => c.Resolve<IServiceFactory>("ServiceFactoryFakeUser").GetArticleService()));
            Container.RegisterType<IArticleService, ArticleServiceAdapter>("ArticleServiceAdapterFakeUser",
                new InjectionFactory(c => new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(), c.Resolve<IContextStorage>())));

            Container.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>(
                new InjectionFactory(
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>())));

            Container.RegisterType<IReadOnlyArticleService, CachedReadOnlyArticleServiceAdapter>("CachedReadOnlyArticleServiceAdapter",
                new InjectionFactory(
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>())));

            Container.RegisterType<IDBConnector, DBConnectorProxy>(new HttpContextLifetimeManager());

            Container.RegisterType<IRegionService, RegionService>("RegionServiceFakeUser",
                    new InjectionFactory(c => new RegionService(Container.Resolve<IVersionedCacheProvider>(),
                        Container.Resolve<ICacheItemWatcher>(),
                        Container.Resolve<IUserProvider>(AlwaysAdminUserProviderName),
                        Container.Resolve<ISettingsService>())));

            Container.RegisterType<IContentDefinitionService, ContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin",
                new InjectionFactory(x => new ContentDefinitionService(
                    x.Resolve<ISettingsService>(),
                    x.Resolve<IVersionedCacheProvider>(),
                    x.Resolve<IArticleService>("ArticleServiceAdapterFakeUser"),
                    x.Resolve<ILogger>(),
                    x.Resolve<IConnectionProvider>())));


            //так как лоадер только читает то нет смысла реальный userId получать и передавать
            Container.RegisterType<IProductService, TLoader>(
                new InjectionConstructor(
                    new ResolvedParameter<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin"),
                    typeof(ILogger),
                    typeof(IVersionedCacheProvider),
                    typeof(ICacheItemWatcher),
                    new ResolvedParameter<IReadOnlyArticleService>("CachedReadOnlyArticleServiceAdapter"),
                    new ResolvedParameter<IFieldService>("FieldServiceAdapterAlwaysAdmin"),
                    typeof(ISettingsService),
                    typeof(IConsumerMonitoringService),
                    typeof(IArticleFormatter),
                    typeof(IConnectionProvider)
                ));

            Container.RegisterType<ILocalizationSettingsService, LocalizationSettingsService>();
            Container.RegisterType<IProductLocalizationService, ProductLocalizationService>();            

            Container.RegisterType<IArticleDependencyService, ArticleDependencyService>(
                new InjectionFactory(c => new ArticleDependencyService(
                    c.Resolve<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin"),
                    c.Resolve<IServiceFactory>("ServiceFactoryFakeUser"),
                    c.Resolve<IVersionedCacheProvider>(),
                    c.Resolve<ISettingsService>(),
                    c.Resolve<IConnectionProvider>())));

            string loaderWarmUpProductIdStr = ConfigurationManager.AppSettings["LoaderWarmUpProductId"];

            if (!string.IsNullOrEmpty(loaderWarmUpProductIdStr))
            {
                int loaderWarmUpProductId = int.Parse(loaderWarmUpProductIdStr);

                Container.RegisterType<IWarmUpProvider, ProductLoaderWarmUpProvider>("ProductLoaderWarmUpProvider", new InjectionConstructor(typeof(ProductLoader), typeof(ILogger), loaderWarmUpProductId));
            }
        }
    }
}
