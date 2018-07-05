using QA.Core.Models.UI;

namespace QA.Core.Models.Tests.Controls
{
    public class QPControlTest : UIControl
    {
        public string HierarchicalMember
        {
            get { return (string)GetValue(HierarchicalMemberProperty); }
            set { SetValue(HierarchicalMemberProperty, value); }
        }

        static QPControlTest() { }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public int ContentId
        {
            get { return (int)GetValue(ContentIdProperty); }
            set { SetValue(ContentIdProperty, value); }
        }



        public static string GetTestProperty(DependencyObject obj)
        {
            return (string)obj.GetValue(TestPropertyProperty);
        }

        public static void SetTestProperty(DependencyObject obj, string value)
        {
            obj.SetValue(TestPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for TestProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TestPropertyProperty =
            DependencyProperty.RegisterAttach("TestProperty", typeof(string), typeof(QPControlTest), inherit: true);

        // Using a DependencyProperty as the backing store for ContentId.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentIdProperty =
            DependencyProperty.Register("ContentId", typeof(int), typeof(QPControlTest));

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(QPControlTest));

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(QPControlTest));

        // Using a DependencyProperty as the backing store for HierarchicalMember.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HierarchicalMemberProperty =
            DependencyProperty.Register("HierarchicalMember", typeof(string), typeof(QPControlTest), inherit: true);
    }
}
