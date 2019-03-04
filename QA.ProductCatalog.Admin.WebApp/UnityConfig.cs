using System;
using System.Linq;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.UI;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Admin.WebApp.App_Core;
using QA.ProductCatalog.Admin.WebApp.Core;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Integration.Configuration;
using QA.ProductCatalog.Validation.Configuration;
using Quantumart.QP8.BLL.Services.API;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.ProductCatalog.Admin.WebApp
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure(IUnityContainer container, LoaderProperties loaderProperties, Properties properties)
        {
            container = RegisterTypes(container, loaderProperties, properties);
            ObjectFactoryConfigurator.DefaultContainer = container;
	        return container;
        }

        public static IUnityContainer RegisterTypes(IUnityContainer container, LoaderProperties loaderProperties, Properties properties)
        {

            container.AddNewExtension<LoaderConfigurationExtension>();
            container.AddNewExtension<ActionContainerConfiguration>();
			container.AddNewExtension<TaskContainerConfiguration>();
			container.AddNewExtension<ValidationConfiguration>();
			
            container.RegisterType<IConnectionProvider, CoreConnectionProvider>();
            container.RegisterType<ICustomerProvider, CustomerProvider>();
            container.RegisterType<IIdentityProvider, IdentityProvider>();


            // логируем в консоль
            //container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));
            container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));

            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();


            container.RegisterType<QPCoreSecurityChecker>();
	        
	        container.RegisterType<DefinitionEditorService>();

            container
                .RegisterType<IXmlProductService, XmlProductService>()
                .RegisterType<IUserProvider, UserProvider>()
                .RegisterType<IQPNotificationService, QPNotificationService>()
                // change default provider to filesystem-based one since it does not require app to recompile on changes.
                // AppDataProductControlProvider does not cache reads from disk
                .RegisterType<IProductControlProvider, ContentBasedProductControlProvider>();

            container.RegisterFactory<CustomActionService>(c => new CustomActionService(c.Resolve<IConnectionProvider>().GetConnection(), 1));

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();

	        container.RegisterType<INotesService, NotesFromContentService>();
            container.RegisterType<IProductUpdateService, ProductUpdateService>();

            //container.RegisterType<IRegionTagReplaceService, RegionTagReplaceService>();

            BindingValueProviderFactory.Current = new DefaultBindingValueProviderFactory(new QPModelBindingValueProvider());


            container.RegisterFactory<TaskRunnerEntities>(c => new TaskRunnerEntities(c.Resolve<IConnectionProvider>().GetEFConnection(Service.Actions)));
            container.RegisterType<ITaskService, TaskService>(new InjectionConstructor(typeof(TaskRunnerEntities)));

            container.RegisterType<IProductRelevanceService, ProductRelevanceService>();

            var entitiesAssembly = typeof(IArticleFilter).Assembly;

            foreach (var filterClass in entitiesAssembly.GetExportedTypes().Where(x => typeof (IArticleFilter).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract))
                container.RegisterType(typeof (IArticleFilter), filterClass, filterClass.Name);


            container.RegisterType<IProductChangeSubscriber, RelevanceUpdaterOnProductChange>("RelevanceUpdaterOnProductChange");


            container.RegisterFactory<IProductChangeNotificator>(x =>
		        {
					var notificator = new ProductChangeNotificator(x.Resolve<ILogger>());

			        notificator.AddSubscribers(x.ResolveAll<IProductChangeSubscriber>());


			        return notificator;
		        }, new ContainerControlledLifetimeManager());

			container.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			container.AddNewExtension<FormattersContainerConfiguration>();
			

	        container.RegisterType<StructureCacheTracker>();
	        
	                    
            container.RegisterType<IWarmUpProvider, ProductLoaderWarmUpProvider>("ProductLoaderWarmUpProvider");
            
            container.RegisterType<WarmUpRepeater>(new SingletonLifetimeManager());
            
            switch (loaderProperties.SettingsSource)
            {
                case SettingsSource.Content:
                    container.RegisterType<ISettingsService, SettingsFromContentCoreService>();
                    break;
                case SettingsSource.AppSettings:
                    container.RegisterType<ISettingsService, SettingsFromQpCoreService>();
                    break;
            }
            
            switch (loaderProperties.DefaultSerializer)
            {
                case DefaultSerializer.Json:
                    container.RegisterType<IArticleFormatter, JsonProductFormatter>();
                    break;
                case DefaultSerializer.Xml:
                    container.RegisterType<IArticleFormatter, XmlProductFormatter>();
                    break;
            }

            var connection = container.Resolve<IConnectionProvider>();

            if (connection.QPMode)
            {
                container
                    .RegisterConsolidationCache(properties.AutoRegisterConsolidationCache)
                    .As<IFactory>()
                    .With<FactoryWatcher>(properties.WatcherInterval)
                    .WithCallback(_factoryWatcher_OnConfigurationModify)
                    .Watch();
            }
            else
            {
                container.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>()
                    .RegisterConsolidationCache(properties.AutoRegisterConsolidationCache, SingleCustomerCoreProvider.Key)
                    .As<IFactory>()
                    .With<FactoryWatcher>()
                    .WithCallback(_factoryWatcher_OnConfigurationModify)	                
                    .Watch();
            }

            if (connection.QPMode || connection.UseQPMonitoring)
            {
                container.RegisterQpMonitoring();
            }
            else
            {
                container.RegisterNonQpMonitoring();
            }

            return container;
        }
        
        private static void _factoryWatcher_OnConfigurationModify(object sender, FactoryWatcherEventArgs e)
        {
            WarmUpHelper.WarmUp();
        }
    }
   
}