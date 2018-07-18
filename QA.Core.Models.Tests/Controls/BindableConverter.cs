using System;
using QA.Core.Models.UI;

namespace QA.Core.Models.Tests.Controls
{

    public class BindableConverter : DependencyObject, IValueConverter
    {
        public object Parameter
        {
            get { return (object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Вот тут Parameter заполняется значением из биндинга
            return string.Format("{0}-{1}", Parameter, value);
        }
        

        // Using a DependencyProperty as the backing store for Parameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(object), typeof(BindableConverter));

    }
}
