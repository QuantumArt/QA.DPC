using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.UI.Controls
{
    public class FileLink : Label
    {
        static FileLink()
        {
            // do not move this line outside of constructor!
            FileProperty = DependencyProperty.Register("File", typeof(PlainArticleField), typeof(FileLink));
        }
        public string GetFileLink()
        {

            return GetUrl(File) ?? "javascript:void();";
        }
        public string TryGetUrl()
        {
            return GetUrl(File) ?? "javascript:void();";
        }

        private static string GetUrl(PlainArticleField field)
        {
            if (field == null) return null;
            var value = field.Value;

            if (value == null) return null;

            if
            (field.FieldId.HasValue &&
                !string.IsNullOrEmpty(field.Value)
                && (field.PlainFieldType == PlainFieldType.File
                    || field.PlainFieldType == PlainFieldType.DynamicImage
                    || field.PlainFieldType == PlainFieldType.Image)
            )
            {
                string baseUri = ObjectFactoryBase.Resolve<IDBConnector>().GetUrlForFileAttribute(field.FieldId.Value);
                return $"{baseUri}/{field.Value}";
            }
            else if (field.PlainFieldType == PlainFieldType.String || field.PlainFieldType == PlainFieldType.StringEnum)
            {
                return field.Value;
            }
            else
            {
                return null;
            }
        }

        public PlainArticleField File
        {
            get { return (PlainArticleField)GetValue(FileProperty); }
            set { SetValue(FileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for File.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FileProperty;
    }
}
