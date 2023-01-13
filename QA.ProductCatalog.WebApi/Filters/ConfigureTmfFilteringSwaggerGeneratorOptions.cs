using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class ConfigureTmfFilteringSwaggerGeneratorOptions : IConfigureOptions<SwaggerGeneratorOptions>
    {
        private readonly TmfSettings _tmfSettings;

        public ConfigureTmfFilteringSwaggerGeneratorOptions(IOptions<TmfSettings> tmfSettingsOptions)
        {
            _tmfSettings = tmfSettingsOptions.Value;
        }

        public void Configure(SwaggerGeneratorOptions options)
        {
            options.DocumentFilters.Add(new TmfAvailabilityFilter(_tmfSettings));
        }
    }
}