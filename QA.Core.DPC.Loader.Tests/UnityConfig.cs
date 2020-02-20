using System;
using System.Configuration;
using Microsoft.AspNetCore.Http;
using Moq;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Core.DPC.Loader.Tests
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            var container = RegisterTypes(new UnityContainer());
            ObjectFactoryConfigurator.DefaultContainer = container;
            return container;
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {
            container.AddExtension(new Diagnostic());
            // логируем в консоль
            container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));
            
            var mock = new Mock<IHttpContextAccessor>();
            container.RegisterInstance(mock.Object);
            container.AddNewExtension<LoaderConfigurationExtension>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();


            // устанавливаем фальшивый сервис для загрузки модели
            container.RegisterType<IProductService, ProductLoader>().RegisterType<IXmlProductService, XmlProductService>();
            container.RegisterType<IQPNotificationService, QPNotificationService>();
            container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<VersionedCacheProviderBase>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
            container.RegisterType<ISettingsService, SettingsFromContentCoreService>();
            container.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
            container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>(), container.Resolve<ILogger>()));
            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();
            container.RegisterType<IConsumerMonitoringService, FakeConsumerMonitoringService>();
            container.RegisterInstance<IConnectionProvider>(new ExplicitConnectionProvider(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString));

            container.RegisterType<IArticleDependencyService, ArticleDependencyService>(
                new InjectionConstructor(
                    typeof(IContentDefinitionService),
                    typeof(IServiceFactory),
                    typeof(VersionedCacheProviderBase),
                    typeof(ISettingsService),
                    typeof(IConnectionProvider)
                    ));

            return container;
        }
    }
}
