using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public static class OptionsContainerBuilderExtensions
    {
        public static ContainerBuilder RegisterOptions(this ContainerBuilder services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.RegisterGeneric(typeof(OptionsManager<>)).As(typeof(IOptions<>)).SingleInstance();
            return services;
        }

        private static bool IsAction(Type type)
        {
            return (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Action<>));
        }

        private static IEnumerable<Type> FindIConfigureOptions(Type type)
        {
            var serviceTypes = type.GetTypeInfo().ImplementedInterfaces
                .Where(t => t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IConfigureOptions<>));
            if (serviceTypes.Any()) return serviceTypes;
            var error = "TODO: No IConfigureOptions<> found.";
            if (IsAction(type))
            {
                error += " did you mean Configure(Action<T>)";
            }
            throw new InvalidOperationException(error);
        }

        public static ContainerBuilder ConfigureOptions(this ContainerBuilder builder, Type configureType)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var serviceTypes = FindIConfigureOptions(configureType);
            foreach (var serviceType in serviceTypes)
            {
                builder.RegisterTransient(serviceType, configureType);
            }
            return builder;
        }

        public static ContainerBuilder ConfigureOptions<TSetup>(this ContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.ConfigureOptions(typeof(TSetup));
        }

        public static ContainerBuilder ConfigureOptions(
            this ContainerBuilder builder, 
            object configureInstance)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configureInstance == null)
            {
                throw new ArgumentNullException(nameof(configureInstance));
            }

            var serviceTypes = FindIConfigureOptions(configureInstance.GetType());
            foreach (var serviceType in serviceTypes)
            {
                builder.RegisterInstance(serviceType, configureInstance);
            }
            return builder;
        }

        public static ContainerBuilder Configure<TOptions>(
            this ContainerBuilder builder,
            Action<TOptions> setupAction)
            where TOptions : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            builder.ConfigureOptions(new ConfigureOptions<TOptions>(setupAction));
            return builder;
        }

        public static ContainerBuilder Configure<TOptions>(
            this ContainerBuilder builder,
            IConfiguration config)
            where TOptions : class
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            builder.ConfigureOptions(
                new ConfigureFromConfigurationOptions<TOptions>(config));

            return builder;
        }
    }
}