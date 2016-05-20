using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
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