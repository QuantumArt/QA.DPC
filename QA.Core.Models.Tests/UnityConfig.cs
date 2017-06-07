using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QA.Core.DPC.Loader;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using QA.Core.Data;
using QA.Core.DPC.Loader.Container;

namespace QA.Core.Models.Tests
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            return ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {
            container.AddNewExtension<LoaderConfigurationExtension>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

            // устанавливаем фальшивый сервис для загрузки модели
            container.RegisterType<IProductService, FakeProductLoader>();
            container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
			container.RegisterType<ISettingsService, SettingsFromContentService>();
			container.RegisterType<IUserProvider, ProductCatalog.Actions.Services.AlwaysAdminUserProvider>();
            container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>()));

			// логируем в консоль
			container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));

            return container;
        }
    }
}
