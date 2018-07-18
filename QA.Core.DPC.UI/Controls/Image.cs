using QA.Core.Models.UI;

namespace QA.Core.DPC.UI.Controls
{
    public class Image : FileLink
    {
        static Image()
        {
            // do not move this line outside of constructor!
            UrlProperty = DependencyProperty.Register("Url", typeof(string), typeof(Image));
        }

        public string Url
        {
            get { return (string)GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public string Width { get; set; }
        public string Height { get; set; }
        public string MaxHeight { get; set; }

        public bool DecorateWithLink { get; set; }

        public static readonly DependencyProperty UrlProperty;
    }
}
