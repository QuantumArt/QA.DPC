﻿using QA.Core.Models;
using QA.Core.Models.UI;
using System;
using System.Globalization;

namespace QA.Core.DPC.UI.Converters
{
    public class NumberFormatConverter : DependencyObject, IValueConverter
    {
        private static readonly DependencyProperty _format;

        static NumberFormatConverter()
        {
            _format = DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumberFormatConverter));
        }

        public string Format
        {
            get => GetValue<string>(_format);
            set => SetValue(_format, value);
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
