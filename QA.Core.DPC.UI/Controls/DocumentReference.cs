using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class DocumentReference : UIElement, ITitled<object>
    {
        public UIElement ReferenceTo { get; set; }

        public object Title
        {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        static DocumentReference() { }

        public object GetTitle()
        {
            if (ReferenceTo == null) return null;

            var title = Title;

            var titled = ReferenceTo as ITitled<object>;
            if (title == null && titled != null)
            {
                return titled.Title ?? title;
            }

            return title;
        }


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(DocumentReference));
    }
}
