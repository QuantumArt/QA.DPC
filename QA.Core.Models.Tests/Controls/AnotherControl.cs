using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.UI;

namespace QA.Core.Models.Tests.Controls
{
    public class AnotherControl : UIControl
    {
        public static string GetColor(DependencyObject obj)
        {
            return (string)obj.GetValue(ColorProperty);
        }

        public static void SetColor(DependencyObject obj, string value)
        {
            obj.SetValue(ColorProperty, value);
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(string), typeof(AnotherControl));
    }
}
