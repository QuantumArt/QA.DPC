using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.DocumentGenerator;
using QA.Core.DPC.API.Container;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.API.Container;
using QA.Core.DPC.QP.Autopublish.Configuration;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.Configuration;
using System;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public static class UnityConfig
	{
		public static IUnityContainer Configure()
		{
		    var container = RegisterTypes(new UnityContainer());
		    ObjectFactoryConfigurator.DefaultContainer = container;
		    return container;
        }

		private static IUnityContainer RegisterTypes(IUnityContainer unityContainer)
		{
            unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
            unityContainer.AddNewExtension<QPContainerConfiguration>();
            unityContainer.AddNewExtension<FormattersContainerConfiguration>();
			unityContainer.AddNewExtension<APIContainerConfiguration>();
            unityContainer.AddNewExtension<QPAPIContainerConfiguration>();

            unityContainer.RegisterType<IUserProvider, ConfigurableUserProvider>();

			unityContainer.RegisterType<ISettingsService, SettingsFromContentService>();

			unityContainer.RegisterType<IRegionTagReplaceService, RegionTagService>();

			unityContainer.RegisterType<IRegionService, RegionService>();

			unityContainer.RegisterType<IXmlProductService, XmlProductService>();

			unityContainer.RegisterType<IContentDefinitionService, ContentDefinitionService>();

			unityContainer.AddNewExtension<LoaderConfigurationExtension>();

			unityContainer.RegisterType<IProductPdfTemplateService, ProductPdfTemplateService>();

			unityContainer.RegisterType<IDocumentGenerator, DocumentGenerator>();

			unityContainer.RegisterType<INotesService, NotesFromContentService>();            

            unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();

			unityContainer.LoadConfiguration("Default");

            unityContainer.RegisterType<StructureCacheTracker>();

            var connection = unityContainer.Resolve<IConnectionProvider>();
		    var logger = unityContainer.Resolve<ILogger>();
            var autoRegister = true;
            var watcherInterval = TimeSpan.FromMinutes(1);

            if (connection.QPMode)
            {
                unityContainer.RegisterConsolidationCache(autoRegister).As<IFactory>().With<FactoryWatcher>(watcherInterval).Watch();
                unityContainer.RegisterQpMonitoring();
            }
            else
            {
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();
                unityContainer.RegisterConsolidationCache(autoRegister, SingleCustomerProvider.Key).As<IFactory>().With<FactoryWatcher>().Watch();
                unityContainer.RegisterNonQpMonitoring();
            }

            return unityContainer;
		}
	}
}