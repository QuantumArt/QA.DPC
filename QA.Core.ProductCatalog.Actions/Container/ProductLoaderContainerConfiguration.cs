using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using System;
using QA.Core.DPC.QP.Cache;
using QA.Core.Logger;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Extension;
using Unity.Lifetime;

namespace QA.Core.ProductCatalog.Actions.Container
{
    public class ProductLoaderContainerConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			// логируем в консоль
			Container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));

			Container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

			Container
				.RegisterType<IXmlProductService, XmlProductService>()
				.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager())
				.RegisterType<VersionedCacheProviderBase>(new ContainerControlledLifetimeManager())
				.RegisterType<IContentInvalidator, DpcContentInvalidator>()
				.RegisterType<ISettingsService, SettingsFromContentCoreService>()
				.RegisterType<IUserProvider, AlwaysAdminUserProvider>()
                .RegisterInstance<ICacheItemWatcher>(new CacheItemWatcherFake())
				.RegisterType<IQPNotificationService, QPNotificationService>()
				.RegisterType<IRegionTagReplaceService, RegionTagService>()
				.RegisterType<IRegionService, RegionService>();
		}
	}
}