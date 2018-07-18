using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class PropertyDisplay : QPControlBase
    {
        public bool Editable { get; set; }
        public QPBehavior Behavior { get; set; }
        public string LabelWidth { get; set; }
        public string ValueWidth { get; set; }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, CorrectValue(value));
            }
        }

        static PropertyDisplay() { }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(PropertyDisplay));
    }
}
