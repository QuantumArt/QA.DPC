using System;
using System.Globalization;

namespace QA.Core.Models
{
	public interface IFixedTypeValueConverter
	{
		object Convert(object value);

		Type OutputType { get; }
	}
}
