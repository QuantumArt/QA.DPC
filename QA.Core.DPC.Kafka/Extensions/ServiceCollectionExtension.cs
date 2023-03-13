using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QA.Core.DPC.Kafka.Models;

namespace QA.Core.DPC.Kafka.Extensions
{
    public static class ServiceCollectionExtension
    {
        /// <summary>
        /// Check if kafka enabled and register kafka provider config.
        /// This method does not register any producers or producer services.
        /// </summary>
        /// <returns></returns>
        public static IServiceCollection RegisterKafka(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaSettings>(configuration.GetSection("Kafka"));

            return services;
        }
    }
}
