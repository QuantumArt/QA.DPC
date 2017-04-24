﻿using System;
using System.Configuration;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.Cache;
using QA.Core.ProductCatalog;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.UI;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.Web;
using QA.ProductCatalog.Admin.WebApp.Core;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Validation.Configuration;
using QA.Core.ProductCatalog.Actions.Container;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Services.API;
using QA.Core.DPC.Loader.Container;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using Task = System.Threading.Tasks.Task;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Xaml;
using Quantumart.QP8.BLL;
using System.Globalization;
using QA.Core.DPC.QP.Servives;
using QA.Core.DPC.QP.Configuration;
using System.Web;
using QA.Core.Data;

namespace QA.ProductCatalog.Admin.WebApp.App_Start
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            var container = ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));

	        Task.Factory.StartNew(() => WarmUpHelper.WarmUp(container));

	        return container;
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {

#if DEBUG
            container.AddNewExtension<LocalSystemCachedLoaderConfigurationExtension>();
#else
            container.AddNewExtension<LoaderConfigurationExtension>();
#endif
            container.AddNewExtension<QPConfigurationExtension>();
            container.AddNewExtension<ActionContainerConfiguration>();
			container.AddNewExtension<TaskContainerConfiguration>();
			container.AddNewExtension<ValidationConfiguration>();

            // логируем в консоль
            //container.RegisterInstance<ILogger>(new TextWriterLogger(Console.Out));
            container.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));

            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();


            container.RegisterType<IAdministrationSecurityChecker, QPSecurityChecker>();
	        
	        container.RegisterType<DefinitionEditorService>();

	        container
                .RegisterType<IXmlProductService, XmlProductService>()
				.RegisterType<ISettingsService, SettingsFromContentService>()
                .RegisterType<IUserProvider, UserProvider>()
                .RegisterType<IQPNotificationService, QPNotificationService>()
                .RegisterType<IProductControlProvider, ProductControlProvider>()
                .RegisterType<IConsumerMonitoringService, ConsumerMonitoringService>(new InjectionConstructor(typeof(IConnectionProvider), true));

            container.RegisterType<CustomActionService>(new InjectionFactory(c => new CustomActionService(c.Resolve<IConnectionProvider>().GetConnection(), 1)));

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();

	        container.RegisterType<INotesService, NotesFromContentService>();

            //container.RegisterType<IRegionTagReplaceService, RegionTagReplaceService>();

            BindingValueProviderFactory.Current = new DefaultBindingValueProviderFactory(new QPModelBindingValueProvider());

            // регистрируем типы для MVC
            container.RegisterType<IControllerActivator, IdentityControllerActivator>(new ContainerControlledLifetimeManager());

            container.RegisterType<TaskRunnerEntities>(new InjectionConstructor());
            container.RegisterType<ITaskService, TaskService>(new InjectionConstructor(typeof(TaskRunnerEntities)));

	        container.RegisterType<Func<bool, IConsumerMonitoringService>>(
		        new InjectionFactory(
			        x =>
				        new Func<bool, IConsumerMonitoringService>(
					        isLive =>
						        new ConsumerMonitoringService(x.Resolve<IConnectionProvider>() ,isLive))));

            container.RegisterType<Func<bool, CultureInfo, IConsumerMonitoringService>>(
               new InjectionFactory(c => new Func<bool, CultureInfo, IConsumerMonitoringService>(
                   (isLive, culture) => new ConsumerMonitoringService(c.Resolve<IConnectionProvider>(), isLive, culture) )));

            container.RegisterType<IProductRelevanceService, ProductRelevanceService>();

            var entitiesAssembly = typeof(IArticleFilter).Assembly;

            foreach (var filterClass in entitiesAssembly.GetExportedTypes().Where(x => typeof (IArticleFilter).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract))
                container.RegisterType(typeof (IArticleFilter), filterClass, filterClass.Name);


            container.RegisterType<IProductChangeSubscriber, RelevanceUpdaterOnProductChange>("RelevanceUpdaterOnProductChange");


            container.RegisterType<IProductChangeNotificator, ProductChangeNotificator>(
		        new ContainerControlledLifetimeManager(),
		        new InjectionFactory(x =>
		        {
					var notificator = new ProductChangeNotificator(x.Resolve<ILogger>());

			        notificator.AddSubscribers(x.ResolveAll<IProductChangeSubscriber>());


			        return notificator;
		        }));

			container.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			container.AddNewExtension<FormattersContainerConfiguration>();
			
            container.LoadConfiguration("Default");

            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(container.Resolve<IControllerActivator>()));

	        container.RegisterType<StructureCacheTracker>();
            

            foreach (var customer in container.Resolve<ICustomerProvider>().GetCustomers())
            {
                var code = customer.CustomerCode;

                var cacheProvider = new VersionedCustomerCacheProvider(code);
                var invalidator = new DPCContentInvalidator(cacheProvider);
                var connectionProvider = new ExplicitConnectionProvider(customer.ConnecdtionString);
                var tracker = new StructureCacheTracker(connectionProvider);
                var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connectionProvider);

                watcher.AttachTracker(tracker);
                
                container.RegisterInstance<IContentInvalidator>(code, invalidator);
                container.RegisterInstance<ICacheProvider>(code, cacheProvider);
                container.RegisterInstance<IVersionedCacheProvider>(code, cacheProvider);
                container.RegisterInstance<ICacheItemWatcher>(code, watcher);              
            }


            container.RegisterType<IContentInvalidator>(new InjectionFactory(c => c.Resolve<IContentInvalidator>(c.Resolve<IIdentityProvider>().Identity.CustomerCode)));
            container.RegisterType<ICacheProvider>(new InjectionFactory(c => c.Resolve<ICacheProvider>(c.Resolve<IIdentityProvider>().Identity.CustomerCode)));
            container.RegisterType<IVersionedCacheProvider>(new InjectionFactory(c => c.Resolve<IVersionedCacheProvider>(c.Resolve<IIdentityProvider>().Identity.CustomerCode)));
            container.RegisterType<ICacheItemWatcher>(new InjectionFactory(c => c.Resolve<ICacheItemWatcher>(c.Resolve<IIdentityProvider>().Identity.CustomerCode)));       

            return container;
        }
    }
}