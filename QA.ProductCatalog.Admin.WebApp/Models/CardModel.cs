using QA.Core.Models.UI;
using System.Globalization;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class CardModel
    {
        public CultureInfo[] Cultures { get; set; }
        public CultureInfo CurrentCultute { get; set; }
        public UIElement Control { get; set; }
    }
}