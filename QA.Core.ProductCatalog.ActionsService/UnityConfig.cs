using System;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.ProductCatalog.TaskScheduler;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using Quartz;
using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Notification.Services;


namespace QA.Core.ProductCatalog.ActionsService
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure()
        {
            return ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));
        }

        public static UnityContainer RegisterTypes(UnityContainer container)
        {
            container.AddNewExtension<ActionContainerConfiguration>();

            container.AddNewExtension<LoaderConfigurationExtension>();

	        container.RegisterType<IUserProvider, UserProvider>(new InjectionConstructor(ConfigurationManager.ConnectionStrings["qp_database"].ConnectionString));

            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

            container.RegisterInstance<ICacheProvider>(container.Resolve<CacheProvider>());

            container.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());

			container.RegisterType<ISettingsService, SettingsFromContentService>();

            container.RegisterType<IContentInvalidator, DPCContentInvalidator>();

            container.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>()));

            container.RegisterType<TaskRunnerEntities>(new InjectionConstructor());

            container.RegisterType<Func<ITaskService>>(new InjectionFactory(x => new Func<ITaskService>(() => x.Resolve<TaskService>())));

            container.RegisterType<ITaskService, TaskService>(new InjectionConstructor(typeof(TaskRunnerEntities)));

            container.RegisterType<Func<string, int, ITask>>(new InjectionFactory(x => new Func<string, int, ITask>((key, userId) => GetTaskByKey(key, userId,x))));

            container.RegisterType<IQPNotificationService, QPNotificationService>();

            container.RegisterType<IRegionTagReplaceService, RegionTagService>();

            container.RegisterType<IRegionService, RegionService>();

            container.RegisterType<IXmlProductService, XmlProductService>();

            container.RegisterType<IProductRelevanceService, ProductRelevanceService>();

			string liveConsumerMonitoringConnString = ConfigurationManager.ConnectionStrings["consumer_monitoring"].ConnectionString;

			string stageConsumerMonitoringConnString = ConfigurationManager.ConnectionStrings["consumer_monitoringStage"].ConnectionString;

			container.RegisterType<Func<bool, IConsumerMonitoringService>>(
				new InjectionFactory(
					x =>
						new Func<bool, IConsumerMonitoringService>(
							isLive =>
								new ConsumerMonitoringService(isLive
									? liveConsumerMonitoringConnString
									: stageConsumerMonitoringConnString))));

			container.RegisterType<IConsumerMonitoringService, ConsumerMonitoringService>(new InjectionConstructor(liveConsumerMonitoringConnString));

            container.RegisterType<ITasksRunner, TasksRunner>();

	        container.RegisterType<ISchedulerFactory, SchedulerFactory>();

			container.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			container.AddNewExtension<FormattersContainerConfiguration>();

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
