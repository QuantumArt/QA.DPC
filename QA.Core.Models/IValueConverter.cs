using System;
using System.Globalization;

namespace QA.Core.Models
{
    public interface IValueConverter
    {
        object Convert(Object value, Type targetType, Object parameter, CultureInfo culture);
    }
}
