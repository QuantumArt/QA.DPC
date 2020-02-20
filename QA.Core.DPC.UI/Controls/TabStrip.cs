#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif
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