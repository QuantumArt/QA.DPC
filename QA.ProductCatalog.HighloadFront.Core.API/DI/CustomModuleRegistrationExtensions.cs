using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    public static class CustomModuleRegistrationExtensions
    {
        private const string ExtraLibrariesSection = "ExtraLibraries";
        private const string ExtraLibraryDependenciesRegistrationEntryPointClass = "Startup";
        private const string ExtraLibraryDependenciesConfigureMethod = "ConfigureServices";
        
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

                    var startup = assembly.GetType($"{extraLibrarySection.Key}.{ExtraLibraryDependenciesRegistrationEntryPointClass}");
                    if (startup == null)
                    {
                        continue;
                    }

                    var configureMethod = startup.GetMethod(ExtraLibraryDependenciesConfigureMethod, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (configureMethod == null)
                    {
                        throw new ApplicationException($"Custom module with root '{ExtraLibraryDependenciesRegistrationEntryPointClass}' class must have '{ExtraLibraryDependenciesConfigureMethod}' instance method inside.");
                    }

                    configureMethod.Invoke(Activator.CreateInstance(startup), new object[] { configuration, services, extraLibrarySection });
                }
            });
        }
    }
}
