using System.Linq;
using System.Windows.Markup;

namespace QA.Core.Models.Configuration
{
    [ContentProperty("Fields")]
	public class VirtualEntityField : BaseVirtualField
	{
		private BaseVirtualField[] _fields;

		public BaseVirtualField[] Fields { get { return _fields ?? new BaseVirtualField[0]; } set { _fields = value; } }

		public override int GetHashCode()
		{
			int currentHash = base.GetHashCode();

			foreach (var field in Fields)
				currentHash = HashHelper.CombineHashCodes(currentHash, field.GetHashCode());

			return currentHash;
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;

			VirtualEntityField otherVirtualField = obj as VirtualEntityField;

			if (otherVirtualField == null)
				return false;

			return Fields.Length == otherVirtualField.Fields.Length && Fields.All(x => otherVirtualField.Fields.Any(y => y.Equals(x)));
		}
	}
}
