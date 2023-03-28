using System;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    public static class CustomModuleRegistrationExtensions
    {
        private const string ExtraLibrariesSection = "ExtraLibraries";
        
        private const string ServicesConfigureGenericMethodName = "Configure";
        private const int ServicesConfigureGenericMethodParamsCount = 2;

        private const string MtsHighloadModuleServiceSuffix = "Service";
        private const string MtsHighloadModuleProcessorSuffix = "Processor";

        public static IMvcBuilder AddCustomModules(this IMvcBuilder mvcBuilder, IConfiguration configuration, IServiceCollection services)
        {
            return mvcBuilder.ConfigureApplicationPartManager(apm =>
            {
                var extraLibrariesSections = configuration.GetSection(ExtraLibrariesSection)?.GetChildren();
                if (!(extraLibrariesSections?.Any() ?? false))
                {
                    return;
                }

                foreach (var extraLibrarySection in extraLibrariesSections)
                {
                    var assembly = Assembly.LoadFile(Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, extraLibrarySection.Key + ".dll"
                    ));

                    apm.ApplicationParts.Add(new AssemblyPart(assembly));

                    RegisterSpecificExtraLibraryDependencies(assembly, services, extraLibrarySection);
                }
            });
        }

        private static void RegisterSpecificExtraLibraryDependencies(
            Assembly assembly,
            IServiceCollection services,
            IConfigurationSection extraLibrarySection)
        {
            switch (assembly.GetName().Name)
            {
                case string name when name.EndsWith("HighloadFront.Core.MTS.API", StringComparison.Ordinal):
                    RegisterMtsHighloadModuleSpecificDependencies(assembly, services, extraLibrarySection);
                    break;
            }
        }

        private static void RegisterMtsHighloadModuleSpecificDependencies(
            Assembly assembly,
            IServiceCollection services,
            IConfigurationSection extraLibrarySection)
        {
            services.AddValidatorsFromAssembly(assembly);

            var assemblyTypes = assembly.GetTypes()
                .Where(t => t.Namespace != null
                    && !t.Namespace.Contains(".Models", StringComparison.Ordinal)
                    && t.IsClass
                    && !t.IsAbstract)
                .ToArray();

            RegisterCustomModuleOptions(assemblyTypes, services, extraLibrarySection);

            var serviceTypesToRegister = assemblyTypes
                .Where(t => t.Name.EndsWith(MtsHighloadModuleServiceSuffix)
                    || t.Name.EndsWith(MtsHighloadModuleProcessorSuffix))
                .ToArray();

            foreach (var serviceType in serviceTypesToRegister)
            {
                if (serviceType.Name.EndsWith(MtsHighloadModuleProcessorSuffix))
                {
                    services.AddSingleton(serviceType);
                }
                else
                {
                    services.AddScoped(serviceType);
                }
            }
        }

        private static void RegisterCustomModuleOptions(
            Type[] assemblyTypes,
            IServiceCollection services,
            IConfigurationSection extraLibrarySection)
        {
            var moduleOptions = assemblyTypes
                .SingleOrDefault(t => t.GetCustomAttribute<CustomModuleOptionsAttribute>() != null);
            if (moduleOptions == null)
            {
                return;
            }

            var configureMethod = typeof(OptionsConfigurationServiceCollectionExtensions).GetMethods()
                .Where(x => ServicesConfigureGenericMethodName.Equals(x.Name, StringComparison.Ordinal))
                .Single(m => m.GetParameters().Length == ServicesConfigureGenericMethodParamsCount)
                .MakeGenericMethod(moduleOptions);
            configureMethod.Invoke(null, new object[] { services, extraLibrarySection });
        }
    }
}
