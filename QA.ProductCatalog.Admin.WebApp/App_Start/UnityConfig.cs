using System;
using System.Configuration;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.Cache;
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
using Task = System.Threading.Tasks.Task;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Xaml;
using System.Globalization;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.Integration.Configuration;
using QA.ProductCatalog.Integration.DAL;

namespace QA.ProductCatalog.Admin.WebApp.App_Start
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            var container = RegisterTypes(new UnityContainer());
            ObjectFactoryConfigurator.DefaultContainer = container;
            Task.Factory.StartNew(() => WarmUpHelper.WarmUp(container));
	        return container;
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {

            container.AddNewExtension<LoaderConfigurationExtension>();
            container.AddNewExtension<QPContainerConfiguration>();
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
                .RegisterType<IProductControlProvider, ProductControlProvider>();

            container.RegisterType<CustomActionService>(new InjectionFactory(c => new CustomActionService(c.Resolve<IConnectionProvider>().GetConnection(), 1)));

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();

	        container.RegisterType<INotesService, NotesFromContentService>();

            //container.RegisterType<IRegionTagReplaceService, RegionTagReplaceService>();

            BindingValueProviderFactory.Current = new DefaultBindingValueProviderFactory(new QPModelBindingValueProvider());

            // регистрируем типы для MVC
            container.RegisterType<IControllerActivator, IdentityControllerActivator>(new ContainerControlledLifetimeManager());

            container.RegisterType<TaskRunnerEntities>(new InjectionFactory(c => new TaskRunnerEntities(c.Resolve<IConnectionProvider>().GetEFConnection(Service.Actions))));
            container.RegisterType<ITaskService, TaskService>(new InjectionConstructor(typeof(TaskRunnerEntities)));

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

            var connection = container.Resolve<IConnectionProvider>();
            var logger = container.Resolve<ILogger>();
            if (connection.QPMode)
            {
                foreach (var customer in container.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;

                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnectionString);
                    var tracker = new StructureCacheTracker(connectionProvider);
                    var watcher =
                        new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15), invalidator, connectionProvider, logger);

                    watcher.AttachTracker(tracker);

                    container.RegisterInstance<IContentInvalidator>(code, invalidator);
                    container.RegisterInstance<ICacheProvider>(code, cacheProvider);
                    container.RegisterInstance<IVersionedCacheProvider>(code, cacheProvider);
                    container.RegisterInstance<ICacheItemWatcher>(code, watcher);

                    watcher.Start();
                }

                container.RegisterType<IContentInvalidator>(
                    new InjectionFactory(c => c.Resolve<IContentInvalidator>(c.GetCustomerCode())));
                container.RegisterType<ICacheProvider>(
                    new InjectionFactory(c => c.Resolve<ICacheProvider>(c.GetCustomerCode())));
                container.RegisterType<IVersionedCacheProvider>(
                    new InjectionFactory(c => c.Resolve<IVersionedCacheProvider>(c.GetCustomerCode())));
                container.RegisterType<ICacheItemWatcher>(
                    new InjectionFactory(c => c.Resolve<ICacheItemWatcher>(c.GetCustomerCode())));

                container.RegisterQpMonitoring();
            }
            else
            {
                container.RegisterInstance<ICacheProvider>(container.Resolve<CacheProvider>());
                container.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(
                    new ContainerControlledLifetimeManager());
                container.RegisterType<IContentInvalidator, DpcContentInvalidator>();

                var watcher =
                    new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15), container.Resolve<IContentInvalidator>(), connection, logger);
                var tracker = new StructureCacheTracker(connection);
                watcher.AttachTracker(tracker);
                watcher.Start();

                container.RegisterInstance<ICacheItemWatcher>(watcher);
                container.RegisterType<ICustomerProvider, SingleCustomerProvider>();

                container.RegisterNonQpMonitoring();
            }


            return container;
        }
    }
}