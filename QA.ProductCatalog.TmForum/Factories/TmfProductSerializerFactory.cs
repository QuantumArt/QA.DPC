using QA.Core.DPC.Front;
using QA.ProductCatalog.ContentProviders;

namespace QA.ProductCatalog.TmForum.Factories;

public class TmfProductSerializerFactory : IProductSerializerFactory
{
    private readonly IProductSerializer _jsonSerializer;
    private readonly IProductSerializer _xmlSerializer;
    private readonly IProductSerializer _tmfSerializer;
    private readonly ISettingsService _settingsService;
    
    public TmfProductSerializerFactory(ISettingsService settingsService,
        TmfProductSerializer tmfSerializer,
        XmlProductSerializer xmlSerializer,
        JsonProductSerializer jsonSerializer)
    {
        _settingsService = settingsService;
        _tmfSerializer = tmfSerializer;
        _xmlSerializer = xmlSerializer;
        _jsonSerializer = jsonSerializer;
    }

    public IProductSerializer Resolve(string format)
    {
        bool tmfEnabledSettingExists = bool.TryParse(_settingsService.GetSetting(SettingsTitles.TMF_ENABLED), out bool tmfEnabled);

        if (format != "json")
        {
            return _xmlSerializer;
        }

        if (tmfEnabledSettingExists && tmfEnabled)
        {
            return _tmfSerializer;
        }

        return _jsonSerializer;

    }
}
