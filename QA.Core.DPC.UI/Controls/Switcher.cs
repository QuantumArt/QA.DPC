using Portable.Xaml.Markup;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    [ContentProperty("Cases")]
    public class Switcher : UIElement
    {
        public object SwitchOn
        {
            get { return (object)GetValue(SwitchOnProperty); }
            set { SetValue(SwitchOnProperty, value); }
        }

        public bool Negate { get; set; }

        public ItemDictionary<object, UIElement> Cases { get; private set; }

        public UIElement Default
        {
            get { return _default; }
            set
            {
                _default = value;
                if (value != null)
                {
                    _default.Parent = this;
                }

            }
        }
        static Switcher() { }
        public Switcher()
        {
            Cases = new ItemDictionary<object, UIElement>(this);
        }

        // Using a DependencyProperty as the backing store for SwitchOn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SwitchOnProperty =
            DependencyProperty.Register("SwitchOn", typeof(object), typeof(Switcher));
        private UIElement _default;


    }
}
