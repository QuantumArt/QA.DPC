using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.Core.Extensions;
using System.ComponentModel;
using QA.Core.Models.Comparers;

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
