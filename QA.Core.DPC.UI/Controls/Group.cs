using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class Group : UIItemsControl, ITitled<object>
    {
        public bool Collapsible { get; set; }
        public bool IsHorizontal { get; set; }

        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, CorrectValue(value)); }
        }

        public bool Collapsed
        {
            get { return (bool)GetValue(CollapsedProperty); }
            set { SetValue(CollapsedProperty, value); }
        }

        static Group() { }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(Group));

        public static readonly DependencyProperty CollapsedProperty =
            DependencyProperty.Register("Collapsed", typeof(bool), typeof(Group));
    }
}
