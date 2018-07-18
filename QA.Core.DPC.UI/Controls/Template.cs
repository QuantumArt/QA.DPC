using System.Collections.Generic;
using System.Windows.Markup;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    [ContentProperty("CellTemplate")]
    public class Template : QPControlBase
    {

        static Template()
        {
            ItemsProperty = DependencyProperty.Register("Items", typeof(IEnumerable<object>), typeof(Template), inherit: false, log: false);
        }

        private UIElement _cellTemplate;

        public UIElement CellTemplate
        {
            get
            {
                if (_cellTemplate != null)
                {
                    object ci = (object)CurrentItem ?? (object)Items;
                    if (ci != null)
                    {
                        _cellTemplate.DataContext = ci;
                    }
                    else
                    {
                        _cellTemplate.DataContext = DataContext;
                    }
                }

                return _cellTemplate;
            }
            set
            {
                _cellTemplate = value;
                if (value != null)
                {
                    _cellTemplate.Parent = this;
                }

            }
        }

        public IEnumerable<object> Items
        {
            get
            {
                var v = (IEnumerable<object>)GetValue(ItemsProperty);
                return v;
            }
            set { SetValue(ItemsProperty, value); }
        }


        // Using a DependencyProperty as the backing store for CurrentItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty;
    }
}
