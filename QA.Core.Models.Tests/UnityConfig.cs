using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using QA.Core.DPC.Loader;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using ILogger = QA.Core.Logger.ILogger;

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
            var mock = new Mock<IHttpContextAccessor>();
            container.RegisterInstance(mock.Object);

            container.RegisterType<ILoggerFactory, LoggerFactory>(
                new ContainerControlledLifetimeManager(), new InjectionConstructor(new ResolvedParameter<IEnumerable<ILoggerProvider>>())
            );
            container.AddExtension(new Diagnostic());
            container.AddNewExtension<LoaderConfigurationExtension>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();
            container.RegisterType<ISettingsService, SettingsService>();
            container.RegisterType<IArticleService, ArticleServiceFake>();
            container.RegisterType<IProductService, FakeProductLoader>();
            container.RegisterType<IConnectionProvider, ConnectionProviderFake>(new InjectionConstructor("server=test;database=testdb"));
            
            container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<VersionedCacheProviderBase>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
			container.RegisterType<IUserProvider, ProductCatalog.Actions.Services.AlwaysAdminUserProvider>();
            container.RegisterInstance<ICacheItemWatcher>(new CacheItemWatcherFake());


			// логируем в консоль
			container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));

            return container;
        }
    }
}
