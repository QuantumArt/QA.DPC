using System.Windows.Markup;

namespace QA.Core.DPC.UI.Controls
{
    [ContentProperty("Title")]
    public class Label : QPControlBase
    {
        public Label()
        {
            HtmlEncode = true;
        }
		public string FontColor { get; set; }

        public string MaxWidth { get; set; }
        public TextOverflow TextOverflow { get; set; }
        public string FontSize { get; set; }
        public string FontWeight { get; set; }

        public bool HtmlEncode { get; set; }
    }
}
