using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI.Controls
{
    public class FileLink : Label
    { 
        public string GetFileLink()
        {
            var field = File;
            if
            (
                field != null &&
                field.FieldId.HasValue &&
                !string.IsNullOrEmpty(field.Value)
                && (field.PlainFieldType == PlainFieldType.File
                    || field.PlainFieldType == PlainFieldType.DynamicImage
                    || field.PlainFieldType == PlainFieldType.Image)
            )
            {
                string baseUri = ObjectFactoryBase.Resolve<IDBConnector>().GetUrlForFileAttribute(field.FieldId.Value);                
                return $"{baseUri}/{field.Value}";
            }
            else
            {
                return "javascript:void();";
            }
        }
    
        public PlainArticleField File
        {
            get { return (PlainArticleField)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for File.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileProperty =
            DependencyProperty.Register("File", typeof(PlainArticleField), typeof(FileLink));
    }
}
