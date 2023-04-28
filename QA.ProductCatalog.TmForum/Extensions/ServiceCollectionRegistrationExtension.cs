using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using QA.Core.Cache;
using QA.Core.DPC.Front;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.TmForum.Factories;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
using QA.ProductCatalog.TmForum.Providers;
using QA.ProductCatalog.TmForum.Services;

namespace QA.ProductCatalog.TmForum.Extensions
{
    public static class ServiceCollectionRegistrationExtension
    {
        /// <summary>
        /// Если в конфигурации параметр Tmf.IsEnabled выставлен в true - дорегистрирует необходимые компоненты в DI.
        /// Если false, то удаляет регистрируемый через метод AddMvc контроллер.
        /// </summary>
        public static IServiceCollection ResolveTmForumRegistration(this IServiceCollection services, IConfiguration configuration)
        {
            TmfSettings settings = GetTmForumSettings(configuration);

            if (!settings.IsEnabled)
            {
                RemoveTmForumFromDI(services);

                return services;
            }

            services.AddSingleton(Options.Create(settings));
            services.AddScoped<ITmfValidatonService, TmfValidationService>();

            return services;
        }

        public static IServiceCollection ResolveTmForumRegistrationForDpcFront(this IServiceCollection services,
            IConfiguration configuration)
        {
            TmfSettings settings = GetTmForumSettings(configuration);

            if (!settings.IsEnabled)
            {
                RemoveTmForumFromDI(services);

                return services;
            }

            services.Configure<ConnectionProperties>(configuration.GetSection("Data"));
            services.Configure<IntegrationProperties>(configuration.GetSection("Integration"));
            services.AddHttpContextAccessor();
            services.TryAddScoped<IIdentityProvider, CoreIdentityProvider>();
            services.TryAddScoped<ICustomerProvider, CustomerProvider>();
            services.TryAddScoped<IConnectionProvider, CoreConnectionProvider>();
            services.AddScoped<VersionedCacheProviderBase>();
            services.AddSingleton<TmfProductSerializer>();

            services.TryAddScoped<ISettingsService>(provider =>
            {
                _ = int.TryParse(configuration["Data:SettingsContentId"], out int result);

                if (result > 0)
                {
                    return new SettingsFromContentCoreService(
                        provider.GetRequiredService<VersionedCacheProviderBase>(),
                        provider.GetRequiredService<IConnectionProvider>(),
                        result);
                }

                return new SettingsFromQpCoreService(
                    provider.GetRequiredService<VersionedCacheProviderBase>(),
                    provider.GetRequiredService<IConnectionProvider>());
            });

            // Replace DPC Front factory with Tmf specific one
            services.Replace(ServiceDescriptor.Scoped<IProductSerializerFactory, TmfProductSerializerFactory>());

            return services;
        }

        public static IServiceCollection ResolveTmForumRegistrationForHighloadApi(this IServiceCollection services, IConfiguration configuration)
        {
            TmfSettings settings = GetTmForumSettings(configuration);

            if (!settings.IsEnabled)
            {
                RemoveTmForumFromDI(services);

                return services;
            }

            services.Replace(ServiceDescriptor.Scoped<IProductInfoProvider>(provider =>
            {
                ISettingsService siteSettings = provider.GetRequiredService<ISettingsService>();

                if (bool.TryParse(siteSettings.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled) && tmfEnabled)
                {
                    return new TmfProductInfoProvider();
                }

                return new ProductInfoProvider();
            }));

            return services;
        }

        private static void RemoveTmForumFromDI(this IServiceCollection services)
        {
            ApplicationPartManager appPartManager = (ApplicationPartManager)services
               .FirstOrDefault(x => x.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance;

            ApplicationPart appPart = appPartManager.ApplicationParts
               .FirstOrDefault(x => x.Name == "QA.ProductCatalog.TmForum");

            if (appPart is not null)
            {
                appPartManager.ApplicationParts.Remove(appPart);
            }
        }

        private static TmfSettings GetTmForumSettings(IConfiguration configuration)
        {
            TmfSettings settings = new();
            configuration.GetSection("Tmf").Bind(settings);

            return settings;
        }
    }
}
