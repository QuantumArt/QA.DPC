using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QA.Core.DPC.Kafka.Extensions;
using QA.Core.DPC.Kafka.Interfaces;
using QA.Core.DPC.Kafka.Services;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
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
            TmfSettings settings = new();
            configuration.GetSection("Tmf").Bind(settings);

            if (settings.IsEnabled)
            {
                services.AddSingleton(Options.Create(settings));
                services.AddScoped<ITmfValidatonService, TmfValidationService>();

                return services;
            }

            ApplicationPartManager appPartManager = (ApplicationPartManager)services
                .FirstOrDefault(x => x.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance;

            ApplicationPart appPart = appPartManager.ApplicationParts
                .FirstOrDefault(x => x.Name == "QA.ProductCatalog.TmForum");

            if (appPart is not null)
            {
                appPartManager.ApplicationParts.Remove(appPart);
            }

            return services;
        }
    }
}
