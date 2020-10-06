using System.Collections.Generic;
using Portable.Xaml.Markup;
using QA.Core.Models.UI;

namespace QA.Core.DPC.UI
{
    [ContentProperty("ItemTemplate")]
    public abstract class QPListControlBase : ItemListControl<UIElement>
    {
        UIElement _itemTemplate;
        public virtual UIElement ItemTemplate
        {
            get
            {
                return _itemTemplate;
            }
            set
            {
                var style = value;

                if (style != null)
                    style.Parent = this;

                _itemTemplate = style;
            }
        }

        public ItemList<UIElement> Children { get; }


        public QPListControlBase()
        {
            Children = new ItemList<UIElement>(this);
        }

        public override IEnumerable<UIElement> GetChildren(IEnumerable<object> items)
        {
            if(Children!=null && Children.Count > 0)
            {
                return Children;
            }
            
            return base.GetChildren(items);
        }

        protected override UIElement GetTemplate()
        {
            return ItemTemplate;
        }

    }
}
