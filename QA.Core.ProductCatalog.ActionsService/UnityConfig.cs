using Microsoft.Practices.Unity.Configuration;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.ProductCatalog.TaskScheduler;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Integration.Configuration;
using Quartz;
using System;
using System.Configuration;
using QA.Core.DPC.QP.Configuration;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Injection;

namespace QA.Core.ProductCatalog.ActionsService
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            var container = RegisterTypes(new UnityContainer());
            ObjectFactoryConfigurator.DefaultContainer = container;
            return container;
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {
            container.AddNewExtension<QPContainerConfiguration>();

            container.AddNewExtension<ActionContainerConfiguration>();

            container.AddNewExtension<LoaderConfigurationExtension>();

	        container.RegisterType<IUserProvider, UserProvider>();

            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

			container.RegisterType<ISettingsService, SettingsFromContentService>();

            container.RegisterType<TaskRunnerEntities>(new InjectionFactory(x => new TaskRunnerEntities(x.Resolve<IConnectionProvider>().GetEFConnection(DPC.QP.Models.Service.Actions))));

            container.RegisterType<Func<ITaskService>>(new InjectionFactory(x => new Func<ITaskService>(() => x.Resolve<ITaskService>())));

            container.RegisterType<ITaskService, TaskService>(new InjectionConstructor(typeof(TaskRunnerEntities)));

            container.RegisterType<Func<string, int, ITask>>(new InjectionFactory(x => new Func<string, int, ITask>((key, userId) => GetTaskByKey(key, userId,x))));

            container.RegisterType<IQPNotificationService, QPNotificationService>();

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();

            container.RegisterType<IRegionService, RegionService>();

            container.RegisterType<IXmlProductService, XmlProductService>();

            container.RegisterType<IProductRelevanceService, ProductRelevanceService>();

            container.RegisterInstance(new TaskRunnerDelays(ConfigurationManager.AppSettings));
            container.RegisterType<ITasksRunner, TasksRunner>();

	        container.RegisterType<ISchedulerFactory, SchedulerFactory>();

			container.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			container.AddNewExtension<FormattersContainerConfiguration>();

            var autoRegister = false;
            var watcherInterval = TimeSpan.FromMinutes(1);

            var connection = container.Resolve<IConnectionProvider>();
            if (connection.QPMode)
            {
                container.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>(watcherInterval).As<IFactoryWatcher>();
                container.RegisterQpMonitoring();
            }
            else
            {                
                container.RegisterType<ICustomerProvider, SingleCustomerProvider>();
                container.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>().As<IFactoryWatcher>();
                container.RegisterNonQpMonitoring();
            }

            container.LoadConfiguration("Default");

            return container;
        }

        private static ITask GetTaskByKey(string key, int userId, IUnityContainer container)
        {
	        UserProvider.ForcedUserId = userId;

			if (container.IsRegistered<ITask>(key))
				return container.Resolve<ITask>(key);
			else
                return (ITask)container.Resolve(Type.GetType(key));
        }
    }
}
