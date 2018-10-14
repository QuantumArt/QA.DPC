using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using System;
using QA.Core.DPC.QP.Cache;
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

			// устанавливаем фальшивый сервис для загрузки модели
			Container.RegisterType<IAdministrationSecurityChecker, FakeAdministrationSecurityChecker>();

			// устанавливаем фальшивый сервис для загрузки модели
				Container
				.RegisterType<IXmlProductService, XmlProductService>()
				.RegisterType<ICacheProvider, CacheProvider>(new ContainerControlledLifetimeManager())
				.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager())
				.RegisterType<IContentInvalidator, DpcContentInvalidator>()
				.RegisterType<ISettingsService, SettingsFromContentService>()
				.RegisterType<IUserProvider, AlwaysAdminUserProvider>()
                .RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, Container.Resolve<IContentInvalidator>(), Container.Resolve<ILogger>()))
				.RegisterType<IQPNotificationService, QPNotificationService>()
				.RegisterType<IRegionTagReplaceService, RegionTagService>()
				.RegisterType<IRegionService, RegionService>();
		}
	}
}