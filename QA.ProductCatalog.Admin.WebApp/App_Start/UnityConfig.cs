using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using QA.Core.DPC.API.Update;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.UI;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.Web;
using QA.ProductCatalog.Admin.WebApp.App_Core;
using QA.ProductCatalog.Admin.WebApp.Core;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Integration.Configuration;
using QA.ProductCatalog.Validation.Configuration;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Linq;
using System.Web.Mvc;
using QA.Core.DPC.QP.Cache;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Task = System.Threading.Tasks.Task;

namespace QA.ProductCatalog.Admin.WebApp.App_Start
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            var container = RegisterTypes(new UnityContainer());
            ObjectFactoryConfigurator.DefaultContainer = container;
            WarmUpHelper.WarmUp();
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
                // change default provider to filesystem-based one since it does not require app to recompile on changes.
                // AppDataProductControlProvider does not cache reads from disk
                .RegisterType<IProductControlProvider, AppDataProductControlProvider>();

            container.RegisterType<CustomActionService>(new InjectionFactory(c => new CustomActionService(c.Resolve<IConnectionProvider>().GetConnection(), 1)));

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();

	        container.RegisterType<INotesService, NotesFromContentService>();
            container.RegisterType<IProductUpdateService, ProductUpdateService>();

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


            container.RegisterType<IProductChangeNotificator>(
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
            var autoRegister = true;
            var watcherInterval = TimeSpan.FromMinutes(1);

            if (connection.QPMode)
            {
                container.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>(watcherInterval).Watch();
            }
            else
            {
                container.RegisterType<ICustomerProvider, SingleCustomerProvider>();
                container.RegisterConsolidationCache(autoRegister, SingleCustomerProvider.Key).With<FactoryWatcher>().Watch();
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
    }
}