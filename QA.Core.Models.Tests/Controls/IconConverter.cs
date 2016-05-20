using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests.Controls
{
    public class IconConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                if (object.Equals(true, value))
                {
                    return "published";
                }
                if (object.Equals(false, value))
                {
                    return "notpublished";
                }
            }

            return "";
        }

        #endregion
    }
}
