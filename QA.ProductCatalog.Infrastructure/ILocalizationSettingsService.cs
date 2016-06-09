using System.Collections.Generic;
using System.Globalization;

namespace QA.ProductCatalog.Infrastructure
{
    public interface ILocalizationSettingsService
    {
        Dictionary<string, CultureInfo> GetSettings(int definitionId);
    }
}
