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
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.Integration.Configuration;
using QA.ProductCatalog.Integration.DAL;

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
            container.AddNewExtension<QPContainerConfiguration>();

            container.AddNewExtension<ActionContainerConfiguration>();

            container.AddNewExtension<LoaderConfigurationExtension>();

	        container.RegisterType<IUserProvider, UserProvider>();

            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();

			container.RegisterType<ISettingsService, SettingsFromContentService>();

            container.RegisterType<TaskRunnerEntities>(new InjectionFactory(x => new TaskRunnerEntities(x.Resolve<IConnectionProvider>().GetEFConnection(DPC.QP.Models.Service.Actions))));

            container.RegisterType<Func<ITaskService>>(new InjectionFactory(x => new Func<ITaskService>(() => x.Resolve<TaskService>())));

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

            var connection = container.Resolve<IConnectionProvider>();
            if (connection.QPMode)
            {
                foreach (var customer in container.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;

                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var logger = container.Resolve<ILogger>();
                    var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnectionString);
                    var tracker = new StructureCacheTracker(connectionProvider);
                    var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connectionProvider, logger);

                    watcher.AttachTracker(tracker);

                    container.RegisterInstance<IContentInvalidator>(code, invalidator);
                    container.RegisterInstance<ICacheProvider>(code, cacheProvider);
                    container.RegisterInstance<IVersionedCacheProvider>(code, cacheProvider);
                    container.RegisterInstance<ICacheItemWatcher>(code, watcher);
                }

                container.RegisterType<IContentInvalidator>(new InjectionFactory(c => c.Resolve<IContentInvalidator>(c.GetCustomerCode())));
                container.RegisterType<ICacheProvider>(new InjectionFactory(c => c.Resolve<ICacheProvider>(c.GetCustomerCode())));
                container.RegisterType<IVersionedCacheProvider>(new InjectionFactory(c => c.Resolve<IVersionedCacheProvider>(c.GetCustomerCode())));
                container.RegisterType<ICacheItemWatcher>(new InjectionFactory(c => c.Resolve<ICacheItemWatcher>(c.GetCustomerCode())));

                container.RegisterQpMonitoring();
            }
            else
            {
                container.RegisterInstance<ICacheProvider>(container.Resolve<CacheProvider>());
                container.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());
                container.RegisterType<IContentInvalidator, DpcContentInvalidator>();


                var watcher = new QP8CacheItemWatcher(InvalidationMode.All, container.Resolve<IContentInvalidator>());
                var tracker = new StructureCacheTracker(connection);
                watcher.AttachTracker(tracker);

                container.RegisterInstance<ICacheItemWatcher>(watcher);
                container.RegisterType<ICustomerProvider, SingleCustomerProvider>();

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
