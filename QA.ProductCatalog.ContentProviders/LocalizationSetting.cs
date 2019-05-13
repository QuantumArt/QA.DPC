using System.Globalization;

namespace QA.ProductCatalog.ContentProviders
{
    public class LocalizationSetting
    {
        public CultureInfo Culture { get; private set; }
        public string Suffix { get; private set; }

        public LocalizationSetting(string lang, string suffix)
        {
            Culture = CultureInfo.GetCultureInfo(lang);
            Suffix = suffix;
        }
    }
}
