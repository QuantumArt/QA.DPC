using System;
using Autofac;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public static class ContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterTransient(this ContainerBuilder builder,
            Type service,
            Type implementationType)
        {
            builder.RegisterType(implementationType)
                .As(service)
                .InstancePerDependency()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterTransient(this ContainerBuilder builder,
            Type service,
            Func<IComponentContext, object> implementationFactory)
        {
            builder.Register(implementationFactory)
                .As(service)
                .InstancePerDependency()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterScoped(this ContainerBuilder builder,
            Type service,
            Type implementationType)
        {
            builder.RegisterType(implementationType)
                .As(service)
                .InstancePerLifetimeScope()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterScoped(this ContainerBuilder builder,
            Type service,
            Func<IComponentContext, object> implementationFactory)
        {
            builder.Register(implementationFactory)
                .As(service)
                .InstancePerLifetimeScope()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterSingleton(this ContainerBuilder builder,
            Type service,
            Type implementationType)
        {
            builder.RegisterType(implementationType)
                .As(service)
                .AsSelf()
                .SingleInstance()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterSingleton(this ContainerBuilder builder,
            Type service,
            Func<IComponentContext, object> implementationFactory)
        {
            builder.Register(implementationFactory)
                .As(service)
                .AsSelf()
                .SingleInstance()
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterInstance(this ContainerBuilder builder,
            Type service,
            object implementationInstance)
        {
            builder.RegisterInstance(implementationInstance)
                .As(service)
                .PreserveExistingDefaults();

            return builder;
        }

        public static ContainerBuilder RegisterTransient<TService, TImplementation>(this ContainerBuilder builder)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterTransient(typeof (TService), typeof (TImplementation));
        }

        public static ContainerBuilder RegisterTransient(this ContainerBuilder builder,
            Type serviceType)
        {
            return builder.RegisterTransient(serviceType, serviceType);
        }

        public static ContainerBuilder RegisterTransient<TService>(this ContainerBuilder builder)
            where TService : class
        {
            return builder.RegisterTransient(typeof (TService));
        }

        public static ContainerBuilder RegisterTransient<TService>(this ContainerBuilder builder,
            Func<IComponentContext, TService> implementationFactory)
            where TService : class
        {
            return builder.RegisterTransient(typeof (TService), implementationFactory);
        }

        public static ContainerBuilder RegisterTransient<TService, TImplementation>(
            this ContainerBuilder builder,
            Func<IComponentContext, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterTransient(typeof (TService), implementationFactory);
        }

        public static ContainerBuilder RegisterScoped<TService, TImplementation>(this ContainerBuilder builder)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterScoped(typeof (TService), typeof (TImplementation));
        }

        public static ContainerBuilder RegisterScoped(this ContainerBuilder builder,
            Type serviceType)
        {
            return builder.RegisterScoped(serviceType, serviceType);
        }

        public static ContainerBuilder RegisterScoped<TService>(this ContainerBuilder builder,
            Func<IComponentContext, TService> implementationFactory)
            where TService : class
        {
            return builder.RegisterScoped(typeof (TService), implementationFactory);
        }

        public static ContainerBuilder RegisterScoped<TService, TImplementation>(
            this ContainerBuilder builder,
            Func<IComponentContext, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterScoped(typeof (TService), implementationFactory);
        }

        public static ContainerBuilder RegisterScoped<TService>(this ContainerBuilder builder)
            where TService : class
        {
            return builder.RegisterScoped(typeof (TService));
        }

        public static ContainerBuilder RegisterSingleton<TService, TImplementation>(this ContainerBuilder builder)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterSingleton(typeof (TService), typeof (TImplementation));
        }

        public static ContainerBuilder RegisterSingleton(this ContainerBuilder builder, Type serviceType)
        {
            return builder.RegisterSingleton(serviceType, serviceType);
        }

        public static ContainerBuilder RegisterSingleton<TService>(this ContainerBuilder builder) 
            where TService : class
        {
            return builder.RegisterSingleton(typeof (TService));
        }

        public static ContainerBuilder RegisterSingleton<TService>(this ContainerBuilder builder, 
            Func<IComponentContext, TService> implementationFactory)
            where TService : class
        {
            return builder.RegisterSingleton(typeof (TService), implementationFactory);
        }

        public static ContainerBuilder RegisterSingleton<TService, TImplementation>(
            this ContainerBuilder builder,
            Func<IComponentContext, TImplementation> implementationFactory)
            where TService : class
            where TImplementation : class, TService
        {
            return builder.RegisterSingleton(typeof (TService), implementationFactory);
        }
    }
}

