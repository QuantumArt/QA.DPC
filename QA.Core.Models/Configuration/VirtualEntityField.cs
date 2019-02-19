using QA.Core.Models.Tools;
using System.Linq;
#if NETSTANDARD
using Portable.Xaml.Markup;
#else
using System.Windows.Markup;
#endif

namespace QA.Core.Models.Configuration
{
	[ContentProperty("Fields")]
	public class VirtualEntityField : BaseVirtualField
	{
		private BaseVirtualField[] _fields;

		public BaseVirtualField[] Fields
        {
            get => _fields ?? new BaseVirtualField[0];
            set { _fields = value; }
        }

        protected override void ShallowCopyMembers(Field field)
        {
            base.ShallowCopyMembers(field);
            var virtualEntityField = (VirtualEntityField)field;

            virtualEntityField._fields = _fields?.ToArray();
        }

        protected override void DeepCopyMembers(Field field, ReferenceDictionary<object, object> visited)
        {
            base.DeepCopyMembers(field, visited);
            var virtualEntityField = (VirtualEntityField)field;

            virtualEntityField._fields = _fields?
                .Select(baseField => (BaseVirtualField)baseField.DeepCopy(visited))
                .ToArray();
        }

        public override int GetHashCode()
		{
			int hash = base.GetHashCode();

			foreach (BaseVirtualField field in Fields)
            {
                hash = HashHelper.CombineHashCodes(hash, field.GetHashCode());
            }

			return hash;
		}

		public override bool Equals(object obj)
		{
            return base.Equals(obj)
                && obj is VirtualEntityField otherVirtualField
                && Fields.Length == otherVirtualField.Fields.Length
                && Fields.All(x => otherVirtualField.Fields.Any(y => y.Equals(x)));
		}
	}
}
