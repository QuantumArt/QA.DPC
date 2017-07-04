using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class Icon : UIElement
    {
        static Icon() { }
        
        public new string ClassName
        {
            get => (string)GetValue(ClassNameProperty);
            set => SetValue(ClassNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for ClassName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClassNameProperty =
            DependencyProperty.Register("ClassName", typeof(string), typeof(Icon));

        
    }
}
