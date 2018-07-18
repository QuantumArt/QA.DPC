using System.Collections.Generic;
using QA.Core.Models.UI;
using System.Windows.Markup;
using QA.Core.DPC.UI.Controls;

namespace QA.Core.DPC.UI
{
    [ContentProperty("Children")]
    public class TabStrip : QPListControlBase
    {
        public TabPosition TabPosition { get; set; }
        public bool Collapsible { get; set; }

        public bool EnableAnimation { get; set; }
        public bool IsFullSized { get; set; }

        public override IEnumerable<UIElement> GetChildren(IEnumerable<object> items)
        {
            return base.GetChildren(items);
        }

        static TabStrip()
        {
        }
    }


    public class TabStripItem : QPControlBase
    {
        public bool IsDefault { get; set; }
    }
}

namespace QA.Core.DPC.UI.Controls
{
    public enum TabPosition
    {
        Top, Left, Right, Bottom
    }
}