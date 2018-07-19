using System.Web.Mvc;
using CommonServiceLocator;
using QA.Configuration;
using QA.Core;
using QA.Core.Web;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;


namespace QA.ProductCatalog.SiteSyncWebHost
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
            // инициализируем ServiceFactory пустым контейнером

            var container = ObjectFactoryConfigurator.Configure();

            ObjectFactoryConfigurator.DefaultContainer = container;

            container = ObjectFactoryConfigurator.DefaultContainer;

            var serviceLocator = new UnityServiceLocator(container);

            // устанавливаем ServiceLocator
            ServiceLocator.SetLocatorProvider(() => serviceLocator);

            // регистрируем типы для MVC
            container.RegisterType<IControllerActivator, UnityControllerActivator>(new ContainerControlledLifetimeManager());

            ControllerBuilder.Current
                .SetControllerFactory(new DefaultControllerFactory(container.Resolve<IControllerActivator>()));

            return container;
        }

        /// <summary>
        /// Регистрация типов по умолчанию
        /// </summary>
        /// <param name="unityContainer"></param>
        public static IUnityContainer RegisterDefaultTypes(this IUnityContainer unityContainer, System.Action dispose = null)
        {
            
            // INFO так проще регистрировать типы на этапе разработки.
            unityContainer.RegisterType<IConfigurationService, ConfigurationService>(new ContainerControlledLifetimeManager());

           
            return unityContainer;
        }

      
    }
}