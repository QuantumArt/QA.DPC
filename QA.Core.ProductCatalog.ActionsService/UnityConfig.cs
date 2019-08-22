﻿using Microsoft.Practices.Unity.Configuration;
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
using Microsoft.Extensions.Configuration;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Configuration;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using Unity;
using Unity.Injection;
using QA.Validation.Xaml.Extensions.Rules;
using QA.Validation.Xaml;

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

            var autoRegister = false;
            var watcherInterval = TimeSpan.FromMinutes(1);

            var connection = container.Resolve<IConnectionProvider>();
            if (connection.QPMode)
            {
                container.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>(watcherInterval).As<IFactoryWatcher>();
            }
            else
            {                
                container.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>();
                container.RegisterConsolidationCache(autoRegister, SingleCustomerCoreProvider.Key).With<FactoryWatcher>().As<IFactoryWatcher>();
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

			if (container.IsRegistered<ITask>(key))
				return container.Resolve<ITask>(key);
			else
                return (ITask)container.Resolve(Type.GetType(key));
        }
    }
}
