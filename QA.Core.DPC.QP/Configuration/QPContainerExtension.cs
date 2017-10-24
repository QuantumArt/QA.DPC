using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Services;
using System;

namespace QA.Core.DPC.QP.Configuration
{
    public static class QPContainerExtension
    {
        public static string GetCustomerCode(this IUnityContainer container)
        {
            return container.Resolve<IIdentityProvider>().Identity.CustomerCode;
        }

        public static void RegisterFactory<F, T>(this IUnityContainer container)
            where F : IFactory<T>
            where T : class
        {
            container.RegisterType<IFactory<T>, F>(new ContainerControlledLifetimeManager());
            container.RegisterType<T>(new InjectionFactory(c => c.Resolve<IFactory<T>>().Create()));
        }

        public static void RegisterFactory<T>(this IUnityContainer container, Func<string, T> factory)
           where T : class
        {
            container.RegisterType<IFactory<T>, CustomFactory<T>>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(c => new CustomFactory<T>(c.Resolve<IIdentityProvider>(), factory))
            );

            container.RegisterType<T>(new InjectionFactory(c => c.Resolve<IFactory<T>>().Create()));
        }

        public static void RegisterFactory<T>(this IUnityContainer container, Func<IUnityContainer, string, T> factory)
          where T : class
        {
            container.RegisterType<IFactory<T>, CustomFactory<T>>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(
                    c => new CustomFactory<T>(c.Resolve<IIdentityProvider>(),
                    customerCode => factory(container, customerCode))
                )
            );

            container.RegisterType<T>(new InjectionFactory(c => c.Resolve<IFactory<T>>().Create()));
        }
    }
}
