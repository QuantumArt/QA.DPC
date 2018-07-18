using System;

namespace QA.Core.Models.UI.Converters
{

    public class ValueExtractorConverter : DependencyObject, IValueConverter
    {
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        static ValueExtractorConverter()
        {
            ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ValueExtractorConverter));
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ctx = DataContext;
            DataContext = value;
            var result = Value;
            DataContext = ctx;
            return result;
        }

        public static readonly DependencyProperty ValueProperty;
    }
}
