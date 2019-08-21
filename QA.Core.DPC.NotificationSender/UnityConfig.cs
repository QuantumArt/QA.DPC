﻿using Microsoft.Practices.Unity.Configuration;
using QA.Core.DPC.DAL;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Container;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.API.Services;
using QA.Core.DPC.QP.Autopublish.Configuration;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;
 using System.Collections.Generic;
 using Microsoft.EntityFrameworkCore;
using QA.Configuration;
using QA.Core.DPC.Service;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using IStatusProvider = QA.ProductCatalog.ContentProviders.IStatusProvider;
using IUserProvider = QA.ProductCatalog.ContentProviders.IUserProvider;
using NotificationChannel = QA.ProductCatalog.ContentProviders.NotificationChannel;
using INotificationChannelService = QA.ProductCatalog.ContentProviders.INotificationChannelService;

namespace QA.Core.DPC
{
    public static class UnityConfig
	{
		public static IUnityContainer Configure(IUnityContainer container, NotificationProperties props)
		{
			container = RegisterTypes(container, props);
			ObjectFactoryConfigurator.DefaultContainer = container;
			return container;
		}


		public static IUnityContainer RegisterTypes(IUnityContainer unityContainer, NotificationProperties props)
		{
			unityContainer.AddExtension(new Diagnostic());
			
			unityContainer.RegisterType<IConnectionProvider, CoreConnectionProvider>();
			unityContainer.RegisterType<ICustomerProvider, CustomerProvider>();
			unityContainer.RegisterType<IIdentityProvider, CoreIdentityProvider>();
			
            unityContainer.RegisterInstance<ILogger>(new NLogLogger("NLogClient.config"));
            unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			unityContainer.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
			unityContainer.RegisterType<IServiceFactory, ServiceFactory>();
			unityContainer.RegisterFactory<ArticleService>(c => c.Resolve<IServiceFactory>().GetArticleService());
			unityContainer.RegisterType<IContextStorage, QpCachedContextStorage>();
            unityContainer.RegisterType<INotificationChannelService, NotificationChannelService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<INotificationService, NotificationService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IStatusProvider, StatusProvider>();
           
            unityContainer.RegisterType<IConfigurationService, ConfigurationService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<IMessageService, MessageService>();
            
            switch (props.SettingsSource)
            {
	            case SettingsSource.Content:
		            unityContainer.RegisterType<ISettingsService, SettingsFromContentCoreService>();
		            break;
	            case SettingsSource.AppSettings:
		            unityContainer.RegisterType<ISettingsService, SettingsFromQpCoreService>();
		            break;
            }
            
            switch (props.ChannelSource)
            {
	            case ChannelSource.Content:
		            unityContainer.RegisterType<INotificationProvider, NotificationContentProvider>();
		            break;
	            case ChannelSource.Configuration:
		            unityContainer.RegisterType<INotificationProvider, NotificationConfigurationProvider>();
		            break;
            }

            unityContainer.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>();          

            var connection = unityContainer.Resolve<IConnectionProvider>();
            var logger = unityContainer.Resolve<ILogger>();
            
            var autoRegister = true;
            var watcherInterval = TimeSpan.FromMinutes(1);

            if (connection.QPMode)
            {
                unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
                unityContainer.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>(watcherInterval).As<IFactoryWatcher>();
            }
            else if (connection.HasConnection(QP.Models.Service.Admin))
            {                
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>();
                unityContainer.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>().As<IFactoryWatcher>();
            }
            else
            {
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerCoreProvider>();
                unityContainer.RegisterNullFactory().With<FactoryWatcher>().As<IFactoryWatcher>();
            }

            return unityContainer;
		}


	}
}