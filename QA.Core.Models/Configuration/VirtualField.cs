using System;
using System.ComponentModel;

namespace QA.Core.Models.Configuration
{
	public sealed class VirtualField : BaseVirtualField
	{
		public string Path { get; set; }

		[DefaultValue(false)]
		public bool PreserveSource { get; set; }

		/// <summary>
		/// путь объекта который надо удалить так как его данные показаны в данном поле
		/// имеет смысл только если отличается от Path
		/// </summary>
		[DefaultValue(null)]
		public string ObjectToRemovePath { get; set; }

		[DefaultValue(null)]
		public IFixedTypeValueConverter Converter { get; set; }

		public override int GetHashCode()
		{
            return HashHelper.CombineHashCodes(
                base.GetHashCode(),
                Path.GetHashCode(),
                (ObjectToRemovePath ?? String.Empty).GetHashCode(),
                Converter == null ? 0 : Converter.GetHashCode());
		}

		public override bool Equals(object obj)
		{
            return base.Equals(obj)
                && obj is VirtualField otherVirtualField
                && otherVirtualField.Path == Path
                && otherVirtualField.ObjectToRemovePath == ObjectToRemovePath
                && otherVirtualField.Converter == Converter;
		}
	}
}
