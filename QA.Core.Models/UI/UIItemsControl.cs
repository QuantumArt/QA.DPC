using Portable.Xaml.Markup;
namespace QA.Core.Models.UI
{
    [ContentProperty("Items")]
    public abstract class UIItemsControl : UIElement
    {
		public virtual ItemList<UIElement> Items { get; private set; }

	    public ElementDisplayMode? DisplayMode { get; set; }

        public UIItemsControl()
            : base()
        {
            Items = new ItemList(this);
        }

        public void AddChild(UIElement element)
        {
            Items.Add(element);
        }

        public void PrependChild(UIElement element)
        {
            Items.Insert(0, element);
        }
    }
}
