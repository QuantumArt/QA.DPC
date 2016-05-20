using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.Cache;
using QA.Core.DocumentGenerator;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.ProductCatalog;
using QA.Core.DPC.API.Container;
using QA.Core.DPC.Formatters.Configuration;
using QA.ProductCatalog.Integration;
using QA.Core.DPC.Notification.Services;

namespace QA.ProductCatalog.WebApi.App_Start
{
	public static class UnityConfig
	{
		public static IUnityContainer Configure()
		{
			return ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));
		}

		private static IUnityContainer RegisterTypes(IUnityContainer unityContainer)
		{
			unityContainer.RegisterConnectionString(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString);

			unityContainer.AddNewExtension<FormattersContainerConfiguration>();
			unityContainer.AddNewExtension<APIContainerConfiguration>();

			unityContainer.RegisterType<IUserProvider, ConfigurableUserProvider>();

			unityContainer.RegisterInstance<ICacheProvider>(unityContainer.Resolve<CacheProvider>());

			unityContainer.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());

			unityContainer.RegisterType<ISettingsService, SettingsFromContentService>();

			unityContainer.RegisterType<IRegionTagReplaceService, RegionTagService>();

			unityContainer.RegisterType<IRegionService, RegionService>();

			unityContainer.RegisterType<IXmlProductService, XmlProductService>();

			unityContainer.RegisterType<IContentInvalidator, DPCContentInvalidator>();

			unityContainer.RegisterType<ICacheItemWatcher, QP8CacheItemWatcher>(
				new ContainerControlledLifetimeManager(),
				new InjectionFactory(container => new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>())));

			unityContainer.RegisterType<IContentDefinitionService, ContentDefinitionService>();

			unityContainer.AddNewExtension<LoaderConfigurationExtension>();

			unityContainer.RegisterType<IProductPdfTemplateService, ProductPdfTemplateService>();

			unityContainer.RegisterType<IDocumentGenerator, DocumentGenerator>();

			unityContainer.RegisterType<INotesService, NotesFromContentService>();

			unityContainer.RegisterType<StructureCacheTracker>(new InjectionConstructor(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString));

			unityContainer.Resolve<ICacheItemWatcher>().AttachTracker(unityContainer.Resolve<StructureCacheTracker>());

			unityContainer.RegisterType<IConsumerMonitoringService, ConsumerMonitoringService>(new InjectionConstructor("none"));

			unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();

			unityContainer.LoadConfiguration("Default");

			return unityContainer;
		}
	}
}