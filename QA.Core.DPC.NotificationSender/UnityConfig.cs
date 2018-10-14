using Microsoft.Practices.Unity.Configuration;
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
using QA.ProductCatalog.Infrastructure;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using IStatusProvider = QA.ProductCatalog.ContentProviders.IStatusProvider;
using IUserProvider = QA.ProductCatalog.ContentProviders.IUserProvider;
using NotificationChannel = QA.ProductCatalog.ContentProviders.NotificationChannel;

namespace QA.Core.DPC
{
    /// <summary>
    /// Конфигуратор unity-контейнера
    /// </summary>
    public static class UnityConfig
	{
		/// <summary>
		/// Настройка unity-контейнера
		/// </summary>
		/// <returns></returns>
		public static IUnityContainer Configure()
		{
		    var container = RegisterTypes(new UnityContainer());
            ObjectFactoryConfigurator.DefaultContainer = container;
		    return container;
		}

		private static IUnityContainer RegisterTypes(UnityContainer unityContainer)
		{
            unityContainer.AddNewExtension<QPContainerConfiguration>();
           
            unityContainer.RegisterType<QA.ProductCatalog.ContentProviders.IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			unityContainer.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
			unityContainer.RegisterType<IServiceFactory, ServiceFactory>();
			unityContainer.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
			unityContainer.RegisterType<IContextStorage, QpCachedContextStorage>();
            unityContainer.RegisterType<INotificationChannelService, NotificationChannelService>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<INotificationService, NotificationService>();
            unityContainer.RegisterType<IStatusProvider, StatusProvider>();

            unityContainer.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>();          

            unityContainer.LoadConfiguration("Default");

            var connection = unityContainer.Resolve<IConnectionProvider>();
            var logger = unityContainer.Resolve<ILogger>();
            unityContainer.RegisterType<NotificationsModelDataContext>(new InjectionFactory(c => new NotificationsModelDataContext(c.Resolve<IConnectionProvider>().GetConnection(QP.Models.Service.Notification))));

            var autoRegister = true;
            var watcherInterval = TimeSpan.FromMinutes(1);

            if (connection.QPMode)
            {
                unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
                unityContainer.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>(watcherInterval).As<IFactoryWatcher>();
            }
            else if (connection.HasConnection(QP.Models.Service.Admin))
            {                
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();
                unityContainer.RegisterConsolidationCache(autoRegister).With<FactoryWatcher>().As<IFactoryWatcher>();
            }
            else
            {
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();
                unityContainer.RegisterNullFactory().With<FactoryWatcher>().As<IFactoryWatcher>();
            }

            return unityContainer;
		}
	}
}