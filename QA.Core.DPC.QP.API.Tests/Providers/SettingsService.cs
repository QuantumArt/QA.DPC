using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.DPC.QP.API.Tests.Providers
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
