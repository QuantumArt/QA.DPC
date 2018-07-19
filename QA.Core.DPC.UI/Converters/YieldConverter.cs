using QA.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace QA.Core.DPC.UI.Converters
{
    public class YieldConverter : IValueConverter
    {
        public static YieldConverter Instance { get; private set; } = new YieldConverter();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return value;

            Type constructedType = typeof(List<>).MakeGenericType(value.GetType());

            var list = Activator.CreateInstance(constructedType);

            if (value != null)
            {
                ((IList)list).Add(value);
            }

            return list;
        }
    }
}
