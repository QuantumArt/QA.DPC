using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Configuration
{
	public class ContentTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(string)) || sourceType.Equals(typeof(int)))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(ITypeDescriptorContext context,
			CultureInfo culture,
			object value)
		{
			if (value is string)
			{
				int id = 0;
				if (int.TryParse(((string)value), out id))
				{
					return new Content { ContentId = id };
				}
				else
				{
					return new Content { ContentName = (string)value };
				}

			}
			else if (value is int)
			{
				return new Content { ContentId = (int)value };
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return false;
		}
	}
}
