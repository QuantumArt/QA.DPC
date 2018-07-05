using QA.Core.Models.Entities;

namespace QA.Core.DPC.UI.Controls.QP
{
    public class FieldValue
    {
        public string fieldName { get; set; }
        public object value { get; set; }

        internal static string GetName(InitFieldValue ifv)
        {
            int? fieldId = ifv.FieldId ?? (ifv.Field as ArticleField)?.FieldId;

            return fieldId.HasValue ?  string.Format("field_{0}", fieldId) : ifv.Field?.ToString();
        }
    }
}
