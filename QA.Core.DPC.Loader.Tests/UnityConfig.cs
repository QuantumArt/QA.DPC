﻿using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;

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
            // логируем в консоль
            container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));
            container.AddNewExtension<LoaderConfigurationExtension>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

            // устанавливаем фальшивый сервис для загрузки модели
            container.RegisterType<IProductService, ProductLoader>().RegisterType<IXmlProductService, XmlProductService>();
            container.RegisterType<IQPNotificationService, QPNotificationService>();
            container.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager());
            container.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());
            container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
            container.RegisterType<ISettingsService, SettingsFromContentService>();
            container.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
            container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>()));
            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();
            container.RegisterType<IConsumerMonitoringService, FakeConsumerMonitoringService>();
            container.RegisterInstance<IConnectionProvider>(new ExplicitConnectionProvider(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString));

            container.RegisterType<IArticleDependencyService, ArticleDependencyService>(
                new InjectionConstructor(
                    typeof(IContentDefinitionService),
                    typeof(IServiceFactory),
                    typeof(IVersionedCacheProvider),
                    typeof(ISettingsService),
                    typeof(IConnectionProvider)
                    ));

            return container;
        }
    }
}
