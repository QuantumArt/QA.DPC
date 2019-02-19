using System;
using System.ComponentModel;

namespace QA.Core.Models.UI.TypeConverters
{
    /// <summary>
    /// "1 5 4 3" -> Padding
    /// "1 5 4" -> Padding
    /// "2 3" -> Padding
    /// </summary>
    public class PaddingTypeConverter: TypeConverter
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
                return null;
            }
          
            if (value is string)
            {              
                var components = ((string)value).Split(new []{ ';', ' '}, StringSplitOptions.RemoveEmptyEntries);
                Rectangle result = new Rectangle(components);
                return result;
            }
           
            return base.ConvertFrom(context, culture, value);
        }
    }
}
