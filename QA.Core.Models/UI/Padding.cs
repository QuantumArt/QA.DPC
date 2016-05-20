using QA.Core.Models.UI.TypeConverters;
using System.ComponentModel;

namespace QA.Core.Models.UI
{
    /// <summary>
    /// Отступы. Можно задавать в виде 10 12 23 34
    /// </summary>
    [TypeConverter(typeof(PaddingTypeConverter))]
    public class Rectangle : DependencyObject
    {
        static Rectangle()
        {
            BottomProperty = DependencyProperty.Register("Bottom", typeof(string), typeof(Rectangle));
        }

        public Rectangle() : base()
        {

        }

        public Rectangle(string[] components) : base()
        {
            if (components.Length == 1)
            {
                Top = Right = Bottom = Left = components[0];
            }
            else if (components.Length == 2)
            {
                Top = Bottom = components[0];
                Right = Left = components[1];
            }
            else if (components.Length == 3)
            {
                Top = components[0];
                Right = Left = components[1];
                Bottom = components[2];
            }
            else if (components.Length == 4)
            {
                Top = components[0];
                Right = components[1];
                Bottom = components[2];
                Left = components[3];
            }
        }

        public string Measure { get; set; } = "px";

        public string Top
        {
            get { return (string)GetValue(TopProperty); }
            set { SetValue(TopProperty, value); }
        }

        public string Left
        {
            get { return (string)GetValue(LeftProperty); }
            set { SetValue(LeftProperty, value); }
        }

        public string Bottom
        {
            get { return (string)GetValue(BottomProperty); }
            set { SetValue(BottomProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Bottom.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BottomProperty;
        public string Right
        {
            get { return (string)GetValue(RightProperty); }
            set { SetValue(RightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Right.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RightProperty =
            DependencyProperty.Register("Right", typeof(string), typeof(Rectangle));

        // Using a DependencyProperty as the backing store for Left.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeftProperty =
            DependencyProperty.Register("Left", typeof(string), typeof(Rectangle));

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopProperty =
            DependencyProperty.Register("Top", typeof(string), typeof(Rectangle));

        public override string ToString()
        {
            return $"{GetText(Top, "top")}{GetText(Right, "right")}{GetText(Bottom, "bottom")}{GetText(Left, "left")} ";
        }

        private string GetText(string component, string name)
        {
            if (!string.IsNullOrEmpty(component))
                return $"padding-{name}: {component}{Measure}; ";

            return string.Empty;
        }
    }
}