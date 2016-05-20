using System;
using Microsoft.Extensions.Configuration;

namespace QA.ProductCatalog.HighloadFront.Infrastructure
{
    public class ConfigureFromConfigurationOptions<TOptions> : ConfigureOptions<TOptions>
        where TOptions : class
    {
        public ConfigureFromConfigurationOptions(IConfiguration config)
            : base(config.Bind)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
        }
    }
}