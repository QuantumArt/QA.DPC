using Microsoft.Extensions.DependencyInjection;
using QA.Core.DPC.Front;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.TmForum.Factories;

public class TmfProductSerializerFactory : IProductSerializerFactory
{
    private readonly ISettingsService _settingsService;
    private readonly IServiceProvider _provider;
    
    public TmfProductSerializerFactory(ISettingsService settingsService, IServiceProvider provider)
    {
        _settingsService = settingsService;
        _provider = provider;
    }

    public IProductSerializer Resolve(string format)
    {
        bool tmfEnabledSettingExists = bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled);

        return format switch
        {
            "json" when tmfEnabledSettingExists && tmfEnabled => _provider.GetRequiredService<TmfProductSerializer>(),
            "json" => _provider.GetRequiredService<JsonProductSerializer>(),
            _ => _provider.GetRequiredService<XmlProductSerializer>()
        };
    }
}
