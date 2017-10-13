using QA.Core.Models;
using QA.Core.Models.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace QA.Core.DPC.UI.Converters
{
    public class StringFormatConverter : DependencyObject, IValueConverter
    {
        static StringFormatConverter()
        {
            FormatProperty = DependencyProperty.Register("Format", typeof(object), typeof(StringFormatConverter));
            Parameter0Property = DependencyProperty.Register("Parameter0", typeof(object), typeof(StringFormatConverter));
            Parameter1Property = DependencyProperty.Register("Parameter1", typeof(object), typeof(StringFormatConverter));
            Parameter2Property = DependencyProperty.Register("Parameter2", typeof(object), typeof(StringFormatConverter));
            Parameter3Property = DependencyProperty.Register("Parameter3", typeof(object), typeof(StringFormatConverter));
            Parameter4Property = DependencyProperty.Register("Parameter4", typeof(object), typeof(StringFormatConverter));
        }

        public object Format
        {
            get { return GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public object Parameter0
        {
            get { return GetValue(Parameter0Property); }
            set { SetValue(Parameter0Property, value); }
        }

        public object Parameter1
        {
            get { return GetValue(Parameter1Property); }
            set { SetValue(Parameter1Property, value); }
        }

        public object Parameter2
        {
            get { return GetValue(Parameter2Property); }
            set { SetValue(Parameter2Property, value); }
        }

        public object Parameter3
        {
            get { return GetValue(Parameter3Property); }
            set { SetValue(Parameter3Property, value); }
        }

        public object Parameter4
        {
            get { return GetValue(Parameter4Property); }
            set { SetValue(Parameter4Property, value); }
        }


        public static readonly DependencyProperty FormatProperty;
        public static readonly DependencyProperty Parameter0Property;
        public static readonly DependencyProperty Parameter1Property;
        public static readonly DependencyProperty Parameter2Property;
        public static readonly DependencyProperty Parameter3Property;
        public static readonly DependencyProperty Parameter4Property;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var paramArray = (new object[5] { Parameter0, Parameter1, Parameter2, Parameter3, Parameter4 }).Where(_ => _ != null).ToArray();
            var format = Format as string;
            if (format == null)
                return null;

            return String.Format(format, paramArray);
        }
    }
}
