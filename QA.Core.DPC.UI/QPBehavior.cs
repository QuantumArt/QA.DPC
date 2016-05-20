using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
