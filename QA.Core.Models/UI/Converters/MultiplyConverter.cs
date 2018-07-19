using System;

namespace QA.Core.Models.UI.Converters
{
    using SystemConvert = System.Convert;
    public class MultiplyConverter : IValueConverter
    {
        public static readonly MultiplyConverter Instance = new MultiplyConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            double initialValue;
            double multiplier;

            if (value is int || value is double)
            {
                initialValue = (double)SystemConvert.ToDouble(value) ;
            }
            else if (!double.TryParse(value.ToString(), out initialValue))
            {
                return value;
            }
            
            if (parameter is int || value is double)
            {
                multiplier = (double)value;
            }
            else if (!double.TryParse(parameter.ToString(), out multiplier))
            {
                return value;
            }

            var result = initialValue * multiplier;

            return SystemConvert.ChangeType(result, targetType);
        }

    }
}
