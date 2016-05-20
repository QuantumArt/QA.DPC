using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace QA.Core.Models.Configuration
{
	public class VirtualMultiEntityField : VirtualEntityField
	{
		public string Path{ get; set; }
		
		public override int GetHashCode()
		{
			return HashHelper.CombineHashCodes(base.GetHashCode(), Path.GetHashCode());
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;

			var otherVirtualField = obj as VirtualMultiEntityField;

			if (otherVirtualField == null)
				return false;

			return otherVirtualField.Path == Path;
		}
	}
}
