using System;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.Models.Tests
{
    public class SettingsService : ISettingsService
    {
        public string GetActionCode(string name)
        {
            throw new NotImplementedException();
        }

        public string GetSetting(SettingsTitles title)
        {
            if (title == SettingsTitles.PRODUCT_TYPES_FIELD_NAME)
            {
                return "Type";
            }

            throw new NotImplementedException();
        }

        public string GetSetting(string title)
        {
            throw new NotImplementedException();
        }
    }
}