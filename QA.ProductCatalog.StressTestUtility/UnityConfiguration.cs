﻿using QA.Core;
using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.QP.Cache;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.Web;
using QA.Core.Web.Qp;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.StressTestUtility.Services;
using Unity;
using Unity.Extension;

namespace QA.ProductCatalog.StressTestUtility
{
    internal class UnityConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.AddNewExtension<LoaderConfigurationExtension>();

			Container.RegisterType<IPublishService, PublishService>();
			Container.RegisterType<IUpdateService, UpdateService>();
			Container.RegisterType<ISimplePublishService, SimplePublishService>();
			Container.RegisterType<IAction, PublishAction>();

			Container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));
			Container.RegisterType<IContentDefinitionService, ContentDefinitionService>();
			Container.RegisterType<IAdministrationSecurityChecker, QPSecurityChecker>();
			Container.RegisterType<IProductService, ProductLoader>();
			Container.RegisterType<IXmlProductService, XmlProductService>();
			Container.RegisterInstance(Container.Resolve<HttpVersionedCacheProvider>());
			Container.RegisterInstance<ICacheProvider>(Container.Resolve<HttpVersionedCacheProvider>());
			Container.RegisterInstance<IVersionedCacheProvider>(Container.Resolve<HttpVersionedCacheProvider>());
			Container.RegisterType<IContentInvalidator, DpcContentInvalidator>();
			Container.RegisterType<ISettingsService, SettingsFromContentService>();
			Container.RegisterType<IUserProvider, TestUserProvider>();
			Container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, Container.Resolve<IContentInvalidator>(), Container.Resolve<ILogger>()));
			Container.RegisterType<IQPNotificationService, QPNotificationService>();
			Container.RegisterType<IConsumerMonitoringService, ConsumerMonitoringService>();
			Container.RegisterType<IRegionTagReplaceService, RegionTagService>();
			Container.RegisterType<IRegionService, RegionService>();

			ObjectFactoryConfigurator.DefaultContainer = Container;
		}
	}
}
