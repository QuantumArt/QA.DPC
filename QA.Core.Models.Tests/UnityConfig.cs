using System;
using QA.Core.DPC.Loader;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.QP.Cache;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Lifetime;

namespace QA.Core.Models.Tests
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
            container.AddNewExtension<LoaderConfigurationExtension>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

            // устанавливаем фальшивый сервис для загрузки модели
            container.RegisterType<IProductService, FakeProductLoader>();
            container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<VersionedCacheProviderBase>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
			container.RegisterType<ISettingsService, SettingsFromContentCoreService>();
			container.RegisterType<IUserProvider, ProductCatalog.Actions.Services.AlwaysAdminUserProvider>();
            container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>(), container.Resolve<ILogger>()));

			// логируем в консоль
			container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));

            return container;
        }
    }
}
