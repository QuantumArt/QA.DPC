using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Extensions;

namespace QA.Core.DPC.UI
{
    public class CollectionConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null)
            {
                return new T[] { };
            }
            if (value is string)
            {
                string text = (string)value;

                var converter = TypeDescriptor.GetConverter(typeof(T));

                return text.SplitString(',', ';')
                    .Select(x => (T)converter.ConvertFromInvariantString(x))
                    .ToArray();
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
