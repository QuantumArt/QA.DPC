using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quantumart.QP8.Configuration;

namespace QA.DPC.Core.Helpers;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Fill QPConfiguration class with ConfigService Url & Token.
    /// Without it publish action will fail when products have QP notifications.
    /// </summary>
    public static IServiceCollection FillQpConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        QPConfiguration.ConfigServiceUrl = configuration.GetSection("Integration").GetValue<string>("ConfigurationServiceUrl");
        QPConfiguration.ConfigServiceToken = configuration.GetSection("Integration").GetValue<string>("ConfigurationServiceToken");
        QPConfiguration.Options = new() { QpConfigPollingInterval = TimeSpan.FromMinutes(1) };

        return services;
    }
}
