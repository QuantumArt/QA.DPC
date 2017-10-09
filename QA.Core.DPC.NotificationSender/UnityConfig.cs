using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core.Cache;
using QA.Core.DPC.DAL;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.API.Services;
using QA.Core.DPC.QP.Autopublish.Configuration;
using QA.Core.DPC.QP.Autopublish.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using System;

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
           
            unityContainer.RegisterType<INotificationAutopublishProvider, NotificationAutopublishProvider>();
            unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
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

            if (connection.QPMode)
            {
                unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
                foreach (var customer in unityContainer.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;
                    
                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnectionString);
                    var watcher = new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromMinutes(1), invalidator, connectionProvider, logger);
                    var tracker = new StructureCacheTracker(connectionProvider);
                    watcher.AttachTracker(tracker);
                    watcher.Start();

                    unityContainer.RegisterInstance<IContentInvalidator>(code, invalidator);
                    unityContainer.RegisterInstance<ICacheProvider>(code, cacheProvider);
                    unityContainer.RegisterInstance<IVersionedCacheProvider>(code, cacheProvider);
                    unityContainer.RegisterInstance<ICacheItemWatcher>(code, watcher);
                }

                unityContainer.RegisterType<IContentInvalidator>(new InjectionFactory(c => c.Resolve<IContentInvalidator>(c.GetCustomerCode())));
                unityContainer.RegisterType<ICacheProvider>(new InjectionFactory(c => c.Resolve<ICacheProvider>(c.GetCustomerCode())));
                unityContainer.RegisterType<IVersionedCacheProvider>(new InjectionFactory(c => c.Resolve<IVersionedCacheProvider>(c.GetCustomerCode())));
                unityContainer.RegisterType<ICacheItemWatcher>(new InjectionFactory(c => c.Resolve<ICacheItemWatcher>(c.GetCustomerCode())));
            }
            else if (connection.HasConnection(QP.Models.Service.Admin))
            {
                var cacheProvider = new VersionedCustomerCacheProvider(null);
                var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                var watcher = new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromMinutes(1), invalidator, connection, logger);
                var tracker = new StructureCacheTracker(connection);                
                watcher.AttachTracker(tracker);
                watcher.Start();

                unityContainer.RegisterInstance<IContentInvalidator>(invalidator);
                unityContainer.RegisterInstance<ICacheProvider>(cacheProvider);
                unityContainer.RegisterInstance<IVersionedCacheProvider>(cacheProvider);
                unityContainer.RegisterInstance<ICacheItemWatcher>(watcher);                
            }
            else
            {
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();
            }

            return unityContainer;
		}
	}
}