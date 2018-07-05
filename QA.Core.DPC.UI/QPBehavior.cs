using System.ComponentModel;

namespace QA.Core.DPC.UI
{
    [TypeConverter(typeof(QPBehaviorTypeConverter))]
    public class QPBehavior
    {
        public bool Editable { get; set; }

        [TypeConverter(typeof(CollectionConverter<string>))]
        public string[] FieldsToHide { get; set; }

        [TypeConverter(typeof(CollectionConverter<string>))]
        public string[] FieldsToBlock { get; set; }
    }
}
