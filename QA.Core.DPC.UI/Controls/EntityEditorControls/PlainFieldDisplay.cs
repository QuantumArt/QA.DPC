using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI.Controls
{
    public class DisplayField : UIElement
    {
        public bool HideEmptyPlainFields { get; set; }

        static DisplayField()
        {
            ValueProperty = DependencyProperty.Register("Value", typeof(PlainArticleField), typeof(DisplayField));
        }

        public PlainArticleField Value
        {
            get { return (PlainArticleField)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string GetFileUrl(PlainArticleField field)
        {
            if (field != null &&
               field.FieldId.HasValue &&
               !string.IsNullOrEmpty(field.Value)
               && (field.PlainFieldType == PlainFieldType.File 
               || field.PlainFieldType == PlainFieldType.DynamicImage 
               || field.PlainFieldType == PlainFieldType.Image))
            {
                string baseUri = ObjectFactoryBase.Resolve<IDBConnector>().GetUrlForFileAttribute(field.FieldId.Value);
                return $"{baseUri}/{field.Value}";
            }
            else
            {
                return "javascript:void();";
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty;

    }
}
