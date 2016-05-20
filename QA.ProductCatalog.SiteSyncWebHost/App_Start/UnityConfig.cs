using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using QA.Configuration;
using QA.Core;
using QA.Core.Service.Interaction;
using QA.Core.Web;


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

            ObjectFactoryConfigurator.InitializeWith(container);

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