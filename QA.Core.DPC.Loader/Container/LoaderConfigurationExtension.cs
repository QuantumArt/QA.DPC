using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Core.DPC.Loader.Container
{
    public class LoaderConfigurationExtension : LoaderConfigurationExtensionBase<ProductLoader> { }
    public class LocalSystemCachedLoaderConfigurationExtension : LoaderConfigurationExtensionBase<LocalSystemCachedLoader> { }

    public class LoaderConfigurationExtensionBase<TLoader> : UnityContainerExtension
        where TLoader : IProductService
    {
        public const string AlwaysAdminUserProviderName = "AlwaysAdminUserProvider";

        public ITypeLifetimeManager GetHttpContextLifeTimeManager()
        {
            return new HttpContextCoreLifetimeManager(Container.Resolve<IHttpContextAccessor>());
        }

        protected override void Initialize()
        {
            Container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));
            Container.RegisterType<IServiceFactory, ServiceFactory>();
            Container.RegisterFactory<ArticleService>(c => c.Resolve<IServiceFactory>().GetArticleService());
            Container.RegisterType<IArticleService, ArticleServiceAdapter>();
            Container.RegisterFactory<FieldService>(c => c.Resolve<IServiceFactory>().GetFieldService());
            Container.RegisterFactory<ContentService>(c => c.Resolve<IServiceFactory>().GetContentService());
            Container.RegisterType<IFieldService, FieldServiceAdapter>(GetHttpContextLifeTimeManager());
            Container.RegisterType<IContentService, ContentServiceAdapter>(GetHttpContextLifeTimeManager());
            Container.RegisterType<IProductContentResolver, ProductContentResolver>();

            Container.RegisterType<IFreezeService, FreezeService>();
            Container.RegisterType<IValidationService, ValidationService>();

            Container.RegisterFactory(typeof(IFieldService), "FieldServiceAdapterAlwaysAdmin", c => new FieldServiceAdapter(
                    new FieldService(c.Resolve<IConnectionProvider>().GetConnection(), 1),
                    c.Resolve<IConnectionProvider>()
                ),
            (IFactoryLifetimeManager)GetHttpContextLifeTimeManager());
            
            Container.RegisterFactory(typeof(IContentService), "ContentServiceAdapterAlwaysAdmin",
                c => new ContentServiceAdapter(
                    new ContentService(c.Resolve<IConnectionProvider>().GetConnection(), 1), 
                    c.Resolve<IConnectionProvider>()
                    ), (IFactoryLifetimeManager)GetHttpContextLifeTimeManager());
            

            Container.RegisterFactory<ITransaction>(c => new Transaction(c.Resolve<IConnectionProvider>(), c.Resolve<ILogger>()));
            Container.RegisterFactory<Func<ITransaction>>(c => new Func<ITransaction>(() => c.Resolve<ITransaction>()));

            Container.RegisterType<IJsonProductService, JsonProductService>();

            Container.RegisterType<IContextStorage, QpCachedContextStorage>();

            Container.RegisterType<IProductDeserializer, ProductDeserializer>();

            //Фейк юзер нужен для работы ремоут валидации. Также нужны варианты сервисов с фейк-юзером
            Container.RegisterType<IUserProvider, AlwaysAdminUserProvider>(AlwaysAdminUserProviderName);
            Container.RegisterFactory<IServiceFactory>(
                "ServiceFactoryFakeUser", 
                c => new ServiceFactory(c.Resolve<IConnectionProvider>(), c.Resolve<IUserProvider>(AlwaysAdminUserProviderName))
            );
            Container.RegisterFactory<ArticleService>(
                "ArticleServiceFakeUser", 
                c => c.Resolve<IServiceFactory>("ServiceFactoryFakeUser").GetArticleService()
             );
            Container.RegisterFactory<IArticleService>("ArticleServiceAdapterFakeUser",
                c => new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(), c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>()));

            Container.RegisterFactory<IReadOnlyArticleService>(
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>()));

            Container.RegisterFactory<IReadOnlyArticleService>("CachedReadOnlyArticleServiceAdapter",
                
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>()));

            Container.RegisterType<IDBConnector, DBConnectorProxy>(GetHttpContextLifeTimeManager());

            Container.RegisterFactory<IRegionService>("RegionServiceFakeUser",
                    c => new RegionService(Container.Resolve<VersionedCacheProviderBase>(),
                        Container.Resolve<ISettingsService>(),
                        Container.Resolve<IConnectionProvider>()));

            Container.RegisterFactory<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin",
                x => new ContentDefinitionService(
                    x.Resolve<ISettingsService>(),
                    x.Resolve<VersionedCacheProviderBase>(),
                    x.Resolve<IArticleService>("ArticleServiceAdapterFakeUser"),
                    x.Resolve<IConnectionProvider>()));


            //так как лоадер только читает то нет смысла реальный userId получать и передавать
            Container.RegisterType<IProductService, TLoader>(
                new InjectionConstructor(
                    new ResolvedParameter<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin"),
                    typeof(VersionedCacheProviderBase),
                    typeof(ICacheItemWatcher),
                    new ResolvedParameter<IReadOnlyArticleService>("CachedReadOnlyArticleServiceAdapter"),
                    new ResolvedParameter<IFieldService>("FieldServiceAdapterAlwaysAdmin"),
                    typeof(ISettingsService),
                    typeof(IList<IConsumerMonitoringService>),
                    typeof(IArticleFormatter),
                    typeof(IConnectionProvider)
                ));

            Container.RegisterType<ILocalizationSettingsService, LocalizationSettingsService>();
            Container.RegisterType<IProductLocalizationService, ProductLocalizationService>();

            Container.RegisterFactory<IArticleDependencyService>(c => new ArticleDependencyService(
                    c.Resolve<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin"),
                    c.Resolve<IServiceFactory>("ServiceFactoryFakeUser"),
                    c.Resolve<VersionedCacheProviderBase>(),
                    c.Resolve<ISettingsService>(),
                    c.Resolve<IConnectionProvider>()));
        }
    }

    public static class CacheExtensions
    {
        public static FactoryBuilder RegisterConsolidationCache(this IUnityContainer container, bool autoRegister, string defaultCode = null)
        {
            return container.RegisterCustomFactory(autoRegister, (context, customer) =>
            {
                var currentCode = defaultCode ?? customer.CustomerCode;

                var logger = container.Resolve<ILogger>();
                var cacheProvider = new VersionedCacheProviderBase(logger);
                
                var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                var connectionProvider = new ExplicitConnectionProvider(customer);
                var tracker = new StructureCacheTracker(customer.ConnectionString, customer.DatabaseType);
                var watcher = new CustomerCoreCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15), invalidator, connectionProvider, logger, databaseType: customer.DatabaseType);

                context.Register<ICacheProvider>(currentCode, cacheProvider);
                context.Register(currentCode, cacheProvider);
                context.Register<IContentInvalidator>(currentCode, invalidator);
                context.Register<ICacheItemWatcher>(currentCode, watcher);

                watcher.AttachTracker(tracker);
                watcher.Start();
            })
            .For<ICacheProvider>(defaultCode)
            .For<VersionedCacheProviderBase>(defaultCode)
            .For<IContentInvalidator>(defaultCode)
            .For<ICacheItemWatcher>(defaultCode);
        }
    }
}
