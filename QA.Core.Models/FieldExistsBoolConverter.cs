using System;

namespace QA.Core.Models
{
    public class FieldExistsBoolConverter : IFixedTypeValueConverter
	{
		public object Convert(object value)
		{
			return value != null;
		}

		public Type OutputType
		{
			get { return typeof (bool); }
		}
	}
}
