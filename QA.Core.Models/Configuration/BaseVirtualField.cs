using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.Configuration
{
	public abstract class BaseVirtualField : Field
	{
		public override int FieldId
		{
			get
			{
				return -1;
			}
		}

		public override int GetHashCode()
		{
			return FieldName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			BaseVirtualField baseVirtualField = obj as BaseVirtualField;

			if (baseVirtualField == null || baseVirtualField.FieldName != FieldName)
				return false;

			return true;
		}
	}
}
