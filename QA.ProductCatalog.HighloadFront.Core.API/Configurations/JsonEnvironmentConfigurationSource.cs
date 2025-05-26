using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using IConfigurationProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;

namespace QA.ProductCatalog.HighloadFront.Core.API.Configurations;

public class JsonEnvironmentConfigurationSource : JsonConfigurationSource
{
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new JsonEnvironmentConfigurationProvider(this);
    }
}