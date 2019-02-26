using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using System;
using System.Threading;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Core.DPC.QP.Configuration
{
    public static class QPContainerExtension
    {
        public static string GetCustomerCode(this IUnityContainer container)
        {
            return container.Resolve<IIdentityProvider>().Identity.CustomerCode ?? SingleCustomerProvider.Key;
        }
    
        public static FactoryBuilder RegisterFactory<T>(this IUnityContainer container, bool autoRegister)
        where T : IFactory
        {
            var factoryName = typeof(T).Name;
            container.RegisterType<IFactory, T>(factoryName, new ContainerControlledLifetimeManager(), new InjectionConstructor(autoRegister));
            var factory = container.Resolve<IFactory>(factoryName);

            return new FactoryBuilder
            {
                Container = container,
                FactoryName = factoryName
            };
        }
        
        public static FactoryBuilder RegisterCustomFactory(this IUnityContainer container, bool autoRegister, Action<IRegistrationContext, string, string> registration)
        {
            var factoryName = $"{nameof(CustomFactory)}_{Guid.NewGuid()}";
            container.RegisterType<IFactory, CustomFactory>(factoryName, new ContainerControlledLifetimeManager(), new InjectionConstructor(registration, typeof(ICustomerProvider), typeof(ILogger), autoRegister));

            return new FactoryBuilder
            {
                Container = container,
                FactoryName = factoryName
            };
        }

        public static FactoryBuilder RegisterNullFactory(this IUnityContainer container)
        {
            return container.RegisterCustomFactory(false, (context, code, connectionString) => { });
        }

        public static FactoryBuilder For<T>(this FactoryBuilder builder, string code = null)
            where T : class
        {
            builder.Container.RegisterType<T>(new InjectionFactory(c => c.Resolve<IFactory>(builder.FactoryName).Resolve<T>(code ?? c.GetCustomerCode())));
            return builder;
        }     

        public static FactoryBuilder As<T>(this FactoryBuilder builder, string name = null)
        {
            if (name == null)
            {
                builder.Container.RegisterType<T>(new InjectionFactory(c => c.Resolve<T>(builder.FactoryName)));
            }
            else
            {
                builder.Container.RegisterType<T>(name, new InjectionFactory(c => c.Resolve<T>(builder.FactoryName)));
            }

            return builder;
        }

        public static FactoryBuilder With<T>(this FactoryBuilder builder, TimeSpan interval)
           where T : IFactoryWatcher
        {
            var factory = builder.Container.Resolve<IFactory>(builder.FactoryName);
            builder.Container.RegisterType<IFactoryWatcher, T>(builder.FactoryName, new ContainerControlledLifetimeManager(), new InjectionConstructor(interval, factory, typeof(ICustomerProvider), typeof(ILogger)));            

            return builder;
        }

        public static FactoryBuilder With<T>(this FactoryBuilder builder)
         where T : IFactoryWatcher
        {
            return builder.With<T>(Timeout.InfiniteTimeSpan);
        }

        public static FactoryBuilder Watch(this FactoryBuilder builder)
        {
            var watcher = builder.Container.Resolve<IFactoryWatcher>(builder.FactoryName);
            watcher.Start();

            return builder;
        }


        public static FactoryBuilder WithCallback(this FactoryBuilder builder, EventHandler<FactoryWatcherEventArgs> eventHandler)
        {
            var watcher = builder.Container.Resolve<IFactoryWatcher>(builder.FactoryName);
            watcher.OnConfigurationModify += eventHandler; 
            return builder;
        }
    }
}
