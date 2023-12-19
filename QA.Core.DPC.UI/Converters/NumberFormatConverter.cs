using QA.Core.Models;
using QA.Core.Models.UI;
using System;
using System.Globalization;

namespace QA.Core.DPC.UI.Converters
{
    public class NumberFormatConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty FormatProperty;

        static NumberFormatConverter()
        {
            FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(NumberFormatConverter));
        }

        public string Format
        {
            get { return GetValue<string>(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string) && !string.IsNullOrWhiteSpace(Format) && value is string stringValue)
            {
                if (decimal.TryParse(stringValue, out decimal number))
                {
                    return number.ToString(Format, culture);
                }
            }

            return value;
        }
    }
}
