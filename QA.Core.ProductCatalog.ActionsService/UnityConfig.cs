using QA.Core.DPC.Formatters.Configuration;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.Actions.Container;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.TaskScheduler;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using QA.ProductCatalog.Integration.Configuration;
using Quartz;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Configuration;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.TmForum.Container;
using Unity;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Core.ProductCatalog.ActionsService
{
    public static class UnityConfig
    {
        public static IUnityContainer Configure(IUnityContainer container, LoaderProperties loaderProperties)
        {
            container = RegisterTypes(container, loaderProperties);
            ObjectFactoryConfigurator.DefaultContainer = container;
            return container;
        }


        public static IUnityContainer RegisterTypes(IUnityContainer container, LoaderProperties loaderProps)
        {
            //fix Diagnostic extension
            container.RegisterType<ILoggerFactory, LoggerFactory>(
                new ContainerControlledLifetimeManager(), new InjectionConstructor(new ResolvedParameter<IEnumerable<ILoggerProvider>>())
            );
            container.AddExtension(new Diagnostic());
            container.RegisterType<DynamicResourceDictionaryContainer>();
            container.RegisterType<ProcessRemoteValidationIf>();
            
            container.RegisterType<IConnectionProvider, CoreConnectionProvider>();
            container.RegisterType<ICustomerProvider, CustomerProvider>();
            container.RegisterType<IIdentityProvider, CoreIdentityProvider>();

            container.AddNewExtension<ActionContainerConfiguration>();
            container.AddNewExtension<LoaderConfigurationExtension>();

            container.RegisterType<ITasksRunner, TasksRunner>();
            container.RegisterType<IUserProvider, HttpContextUserProvider>();
            container.RegisterType<IContentDefinitionService, ContentDefinitionService>();
            
            container.RegisterFactory<Func<ITaskService>>(x => new Func<ITaskService>(() => x.Resolve<ITaskService>()));

            container.RegisterFactory<Func<string, int, ITask>>(x => new Func<string, int, ITask>((key, userId) => GetTaskByKey(key, userId,x)));
            container.RegisterType<ITaskService, TaskService>();
            
            container.RegisterType<IQPNotificationService, QPNotificationService>();
            container.RegisterType<IRegionTagReplaceService, RegionTagService>();
            container.RegisterType<IRegionService, RegionService>();
            container.RegisterType<IXmlProductService, XmlProductService>();
            container.RegisterType<IProductRelevanceService, ProductRelevanceService>();
	        container.RegisterType<ISchedulerFactory, SchedulerFactory>();

            container.RegisterSingleton<ActionsService>();
			container.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			container.AddNewExtension<FormattersContainerConfiguration>();
            container.AddNewExtension<TmfConfigurationExtension>();

            var autoRegister = false;
            var watcherInterval = TimeSpan.FromMinutes(1);

            var connection = container.Resolve<IConnectionProvider>();
            if (connection.QPMode)
            {
                container.RegisterConsolidationCache(autoRegister).As<IFactory>().With<FactoryWatcher>(watcherInterval).As<IFactoryWatcher>();
            }
            else
            {                
                container.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>();
                container.RegisterConsolidationCache(autoRegister, SingleCustomerCoreProvider.Key).As<IFactory>().With<FactoryWatcher>().As<IFactoryWatcher>();
            }

            if (connection.QPMode || connection.UseQPMonitoring)
            {
                container.RegisterQpMonitoring();
            }
            else
            {
                container.RegisterNonQpMonitoring();        
            }

            switch (loaderProps.SettingsSource)
            {
                case SettingsSource.Content:
                    container.RegisterType<ISettingsService, SettingsFromContentCoreService>();
                    break;
                case SettingsSource.AppSettings:
                    container.RegisterType<ISettingsService, SettingsFromQpCoreService>();
                    break;
            }
            
            switch (loaderProps.DefaultSerializer)
            {
                case DefaultSerializer.Json:
                    container.RegisterType<IArticleFormatter, JsonProductFormatter>();
                    break;
                case DefaultSerializer.Xml:
                    container.RegisterType<IArticleFormatter, XmlProductFormatter>();
                    break;
            }

            return container;
        }

        private static ITask GetTaskByKey(string key, int userId, IUnityContainer container)
        {
	        HttpContextUserProvider.ForcedUserId = userId;
            
            ITask result = null;
            if (container.IsRegistered<ITask>(key))
            {
                result = container.Resolve<ITask>(key);
            }
            else
                result = (ITask)container.Resolve(Type.GetType(key));

            HttpContextUserProvider.ForcedUserId = 0;

            return result;
        }
    }
}
