﻿using Microsoft.Practices.Unity;
using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using Microsoft.Practices.Unity.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Notification.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using QA.Core.DPC.QP.Configuration;
using QA.Core.DPC.QP.Servives;
using QA.Core.DPC.DAL;

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
			
			unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			unityContainer.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
			unityContainer.RegisterType<IServiceFactory, ServiceFactory>();
			unityContainer.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
			unityContainer.RegisterType<IContextStorage, QpCachedContextStorage>();
            unityContainer.RegisterType<INotificationChannelService, NotificationChannelService>(new ContainerControlledLifetimeManager());

			unityContainer.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>();          

            unityContainer.LoadConfiguration("Default");

            var connection = unityContainer.Resolve<IConnectionProvider>();
            unityContainer.RegisterType<NotificationsModelDataContext>(new InjectionFactory(c => new NotificationsModelDataContext(c.Resolve<IConnectionProvider>().GetConnection(QP.Models.Service.Notification))));

            if (connection.QPMode)
            {
                foreach (var customer in unityContainer.Resolve<ICustomerProvider>().GetCustomers())
                {
                    var code = customer.CustomerCode;

                    var cacheProvider = new VersionedCustomerCacheProvider(code);
                    var invalidator = new DPCContentInvalidator(cacheProvider);
                    var connectionProvider = new ExplicitConnectionProvider(customer.ConnecdtionString);
                    var watcher = new CustomerQP8CacheItemWatcher(InvalidationMode.All, invalidator, connectionProvider);

                    var tracker = new StructureCacheTracker(connectionProvider);
                    watcher.AttachTracker(tracker);

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
                unityContainer.RegisterType<IContentInvalidator, DPCContentInvalidator>();
                unityContainer.RegisterInstance<ICacheProvider>(unityContainer.Resolve<CacheProvider>());
                unityContainer.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());
                unityContainer.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, unityContainer.Resolve<IContentInvalidator>()));
            }

            return unityContainer;
		}
	}
}