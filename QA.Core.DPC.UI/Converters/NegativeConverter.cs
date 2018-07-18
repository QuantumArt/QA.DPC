using QA.Core.Models;
using System;
using System.Globalization;

namespace QA.Core.DPC.UI.Converters
{
    public class NegativeConverter : IValueConverter
    {
        public static NegativeConverter Default = new NegativeConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool)
            {
                return !(bool)value;
            }

            return null;
        }
    }
}
