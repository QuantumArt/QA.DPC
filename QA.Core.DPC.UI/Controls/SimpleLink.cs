using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class SimpleLink : QPControlBase
    {
        static SimpleLink()
        {
            HrefProperty = DependencyProperty.Register("Href", typeof(string), typeof(SimpleLink));
        }

        public bool OpenInNewWindow { get; set; } = true;
        public string IconClass { get; set; } 

        public string Href
        {
            get { return (string)GetValue(HrefProperty); }
            set { SetValue(HrefProperty, value); }
        }

        public static readonly DependencyProperty HrefProperty;
    }
}
