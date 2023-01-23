using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QA.ProductCatalog.TmForum.Extensions
{
    public static class ServiceCollectionRegistrationExtension
    {
        /// <summary>
        /// Удаляет контроллер и всё что было загружено из сборки QA.ProductCatalog.TmForum при работе метода AddMvc
        /// в случае если в конфигурации Tmf.IsEnabled выставлен в false.
        /// </summary>
        public static IServiceCollection TryUnregisterTmForum(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetSection("Tmf").GetSection("IsEnabled").Get<bool>())
            {
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
