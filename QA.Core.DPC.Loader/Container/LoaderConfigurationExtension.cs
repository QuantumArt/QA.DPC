using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Extension;
using Unity.Injection;

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
            Container.RegisterType<ContentService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetContentService()));
            Container.RegisterType<IFieldService, FieldServiceAdapter>(new HttpContextLifetimeManager());
            Container.RegisterType<IContentService, ContentServiceAdapter>(new HttpContextLifetimeManager());
            Container.RegisterType<IProductContentResolver, ProductContentResolver>();

            Container.RegisterType<IFreezeService, FreezeService>();

            Container.RegisterType<IFieldService>("FieldServiceAdapterAlwaysAdmin",
                new HttpContextLifetimeManager(),
                new InjectionFactory(x => new FieldServiceAdapter(new FieldService(x.Resolve<IConnectionProvider>().GetConnection(), 1), x.Resolve<IConnectionProvider>())));

            Container.RegisterType<ITransaction>(new InjectionFactory(c => new Transaction(c.Resolve<IConnectionProvider>(), c.Resolve<ILogger>())));
            Container.RegisterType<Func<ITransaction>>(new InjectionFactory(c => new Func<ITransaction>(() => c.Resolve<ITransaction>())));

            Container.RegisterType<IJsonProductService, JsonProductService>();

            Container.RegisterType<IContextStorage, QpCachedContextStorage>();

            Container.RegisterType<IProductDeserializer, ProductDeserializer>();

            //Фейк юзер нужен для работы ремоут валидации. Также нужны варианты сервисов с фейк-юзером
            Container.RegisterType<IUserProvider, AlwaysAdminUserProvider>(AlwaysAdminUserProviderName);
            Container.RegisterType<IServiceFactory>("ServiceFactoryFakeUser", new InjectionFactory(c => new ServiceFactory(c.Resolve<IConnectionProvider>(), c.Resolve<IUserProvider>(AlwaysAdminUserProviderName))));
            Container.RegisterType<ArticleService>("ArticleServiceFakeUser", new InjectionFactory(c => c.Resolve<IServiceFactory>("ServiceFactoryFakeUser").GetArticleService()));
            Container.RegisterType<IArticleService>("ArticleServiceAdapterFakeUser",
                new InjectionFactory(c => new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(), c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>())));

            Container.RegisterType<IReadOnlyArticleService>(
                new InjectionFactory(
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>())));

            Container.RegisterType<IReadOnlyArticleService>("CachedReadOnlyArticleServiceAdapter",
                new InjectionFactory(
                    c =>
                        new ArticleServiceAdapter(c.Resolve<ArticleService>("ArticleServiceFakeUser"), c.Resolve<IConnectionProvider>(),
                            c.Resolve<IContextStorage>(), c.Resolve<IIdentityProvider>())));

            Container.RegisterType<IDBConnector, DBConnectorProxy>(new HttpContextLifetimeManager());

            Container.RegisterType<IRegionService>("RegionServiceFakeUser",
                    new InjectionFactory(c => new RegionService(Container.Resolve<IVersionedCacheProvider>(),
                        Container.Resolve<ISettingsService>(),
                        Container.Resolve<IConnectionProvider>())));

            Container.RegisterType<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin",
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
                    typeof(IList<IConsumerMonitoringService>),
                    typeof(IArticleFormatter),
                    typeof(IConnectionProvider)
                ));

            Container.RegisterType<ILocalizationSettingsService, LocalizationSettingsService>();
            Container.RegisterType<IProductLocalizationService, ProductLocalizationService>();

            Container.RegisterType<IArticleDependencyService>(
                new InjectionFactory(c => new ArticleDependencyService(
                    c.Resolve<IContentDefinitionService>("ContentDefinitionServiceAlwaysAdmin"),
                    c.Resolve<IServiceFactory>("ServiceFactoryFakeUser"),
                    c.Resolve<IVersionedCacheProvider>(),
                    c.Resolve<ISettingsService>(),
                    c.Resolve<IConnectionProvider>())));

            if (int.TryParse(ConfigurationManager.AppSettings["LoaderWarmUpProductId"], out var loaderWarmUpProductId))
            {
                Container.RegisterType<IWarmUpProvider, ProductLoaderWarmUpProvider>("ProductLoaderWarmUpProvider", 
                    new InjectionConstructor(typeof(ProductLoader), typeof(ILogger), loaderWarmUpProductId));

                if (int.TryParse(ConfigurationManager.AppSettings["LoaderWarmUpRepeatInMinutes"],
                    out var loaderWarmUpRepeatInMinutes))
                {
                    Container.RegisterInstance(new WarmUpRepeater(loaderWarmUpRepeatInMinutes));
                }
            }
        }
    }

    public static class CacheExtensions
    {
        public static FactoryBuilder RegisterConsolidationCache(this IUnityContainer container, bool autoRegister, string defaultCode = null)
        {
            return container.RegisterCustomFactory(autoRegister, (context, code, connectionString) =>
            {
                var currentCode = defaultCode ?? code;

                var logger = container.Resolve<ILogger>();
                var cacheProvider = new VersionedCustomerCacheProvider(currentCode);
                var newCacheProvider = new VersionedCacheProviderBase(logger);
                
                var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                var connectionProvider = new ExplicitConnectionProvider(connectionString);
                var tracker = new StructureCacheTracker(connectionProvider);
                var watcher = new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15), invalidator, connectionProvider, logger);

                context.Register<ICacheProvider>(currentCode, cacheProvider);
                context.Register<IVersionedCacheProvider>(currentCode, cacheProvider);
                context.Register<IVersionedCacheProvider2>(currentCode, newCacheProvider);                
                context.Register<IContentInvalidator>(currentCode, invalidator);
                context.Register<ICacheItemWatcher>(currentCode, watcher);

                watcher.AttachTracker(tracker);
                watcher.Start();
            })
            .For<ICacheProvider>(defaultCode)
            .For<IVersionedCacheProvider>(defaultCode)
            .For<IVersionedCacheProvider2>(defaultCode)
            .For<IContentInvalidator>(defaultCode)
            .For<ICacheItemWatcher>(defaultCode);
        }
    }
}
