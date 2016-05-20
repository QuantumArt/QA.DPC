using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
