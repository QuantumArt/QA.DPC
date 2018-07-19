using System;

namespace QA.Core.Models.UI.Converters
{
    public class StringLowerCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            return value.ToString().ToLower();
        }

    }
}
