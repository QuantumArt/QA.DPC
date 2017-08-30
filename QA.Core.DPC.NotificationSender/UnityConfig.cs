using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core.Cache;
using QA.Core.DPC.DAL;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Notification.Services;
using QA.Core.DPC.QP.Autopublish.Configuration;
using QA.Core.DPC.QP.Autopublish.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Services;
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
			return ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));
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

            unityContainer.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>();          

            unityContainer.LoadConfiguration("Default");

            var connection = unityContainer.Resolve<IConnectionProvider>();
            unityContainer.RegisterType<NotificationsModelDataContext>(new InjectionFactory(c => new NotificationsModelDataContext(c.Resolve<IConnectionProvider>().GetConnection(QP.Models.Service.Notification))));

            if (connection.QPMode)
            {
                unityContainer.AddNewExtension<QPAutopublishContainerConfiguration>();
                foreach (var customer in unityContainer.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;

                    var logger = unityContainer.Resolve<ILogger>();
                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var invalidator = new DpcContentInvalidator(cacheProvider, logger);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnectionString);
                    
                    var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connectionProvider, logger, TimeSpan.FromMinutes(1), 1000);
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
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();

                unityContainer.RegisterType<IContentInvalidator, DpcContentInvalidator>();
                unityContainer.RegisterInstance<ICacheProvider>(unityContainer.Resolve<CacheProvider>());
                unityContainer.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());
                unityContainer.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, unityContainer.Resolve<IContentInvalidator>()));
            }
            else
            {
                unityContainer.RegisterType<ICustomerProvider, SingleCustomerProvider>();
            }

            return unityContainer;
		}
	}
}