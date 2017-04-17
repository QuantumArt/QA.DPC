using Microsoft.Practices.Unity;
using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using System.Configuration;
using Microsoft.Practices.Unity.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.Notification.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

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
			var connection = ConfigurationManager.ConnectionStrings["qp_database"];
			string connectionString = connection == null ? null : connection.ConnectionString;
			
			unityContainer.RegisterType<IContentProvider<NotificationChannel>, NotificationChannelProvider>();
			unityContainer.RegisterType<IUserProvider, AlwaysAdminUserProvider>();
			unityContainer.RegisterType<IServiceFactory, ServiceFactory>();
			unityContainer.RegisterType<ArticleService>(new InjectionFactory(c => c.Resolve<IServiceFactory>().GetArticleService()));
			unityContainer.RegisterType<IContextStorage, QpCachedContextStorage>();
            unityContainer.RegisterType<INotificationChannelService, NotificationChannelService>(new ContainerControlledLifetimeManager());

			unityContainer.RegisterType<IReadOnlyArticleService, ReadOnlyArticleServiceAdapter>();

			unityContainer.RegisterType<IContentInvalidator, DPCContentInvalidator>();
			unityContainer.RegisterType<IVersionedCacheProvider, VersionedCacheProvider3>(new ContainerControlledLifetimeManager());

			if (connectionString != null)
			{
				unityContainer.RegisterInstance<ICacheProvider>(unityContainer.Resolve<CacheProvider>());
				unityContainer.RegisterInstance<ICacheItemWatcher>(new QP8CacheItemWatcher(InvalidationMode.All, unityContainer.Resolve<IContentInvalidator>()));
			}
			
			unityContainer.LoadConfiguration("Default");

			return unityContainer;
		}
	}
}