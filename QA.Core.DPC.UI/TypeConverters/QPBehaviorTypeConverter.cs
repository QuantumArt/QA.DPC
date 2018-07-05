using System;
using System.ComponentModel;

namespace QA.Core.DPC.UI
{
    public class QPBehaviorTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            if (sourceType == typeof(bool))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null)
            {
                return null;
            }
            if (value is string)
            {
                string text = (string)value;
                bool val;
                if (bool.TryParse(text, out val))
                {
                    return new QPBehavior { Editable = val };
                }
            }
            if (value is bool)
            {
                return new QPBehavior { Editable = (bool)value };
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
