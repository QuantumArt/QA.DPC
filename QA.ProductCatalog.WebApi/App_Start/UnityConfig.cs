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
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Servives;

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
            unityContainer.AddNewExtension<QPContainerConfiguration>();
            unityContainer.AddNewExtension<FormattersContainerConfiguration>();
			unityContainer.AddNewExtension<APIContainerConfiguration>();

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
		
            unityContainer.RegisterType<IConsumerMonitoringService, ConsumerMonitoringService>(new InjectionConstructor(typeof(IConnectionProvider), true));

            unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();

			unityContainer.LoadConfiguration("Default");

            unityContainer.RegisterType<StructureCacheTracker>();

            var connection = unityContainer.Resolve<IConnectionProvider>();

            if (connection.QPMode)
            {
                foreach (var customer in unityContainer.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;

                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var invalidator = new DPCContentInvalidator(cacheProvider);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnectionString);
                    var tracker = new StructureCacheTracker(connectionProvider);
                    var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connectionProvider);

                    watcher.AttachTracker(tracker);

                    unityContainer.RegisterInstance<IContentInvalidator>(code, invalidator);
                    unityContainer.RegisterInstance<ICacheProvider>(code, cacheProvider);
                    unityContainer.RegisterInstance<IVersionedCacheProvider>(code, cacheProvider);
                    unityContainer.RegisterInstance<ICacheItemWatcher>(code, watcher);
                }

                unityContainer.RegisterType<IContentInvalidator>(new InjectionFactory(c => c.Resolve<IContentInvalidator>(c.GetCustomerCode())));
                unityContainer.RegisterType<ICacheProvider>(new InjectionFactory(c => c.Resolve<ICacheProvider>(c.GetCustomerCode())));
                unityContainer.RegisterType<IVersionedCacheProvider>(new InjectionFactory(c => c.Resolve<IVersionedCacheProvider>(c.GetCustomerCode())));
                unityContainer.RegisterType<ICacheItemWatcher>(new InjectionFactory(c => c.Resolve<ICacheItemWatcher>(c.GetCustomerCode())));
            }
            else
            {
                var cacheProvider = new VersionedCustomerCacheProvider(null);
                var invalidator = new DPCContentInvalidator(cacheProvider);
                var tracker = new StructureCacheTracker(connection);
                var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connection);

                watcher.AttachTracker(tracker);

                unityContainer.RegisterInstance<IContentInvalidator>(invalidator);
                unityContainer.RegisterInstance<ICacheProvider>(cacheProvider);
                unityContainer.RegisterInstance<IVersionedCacheProvider>(cacheProvider);
                unityContainer.RegisterInstance<ICacheItemWatcher>(watcher);
            }

            return unityContainer;
		}
	}
}