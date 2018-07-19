namespace QA.Core.Models.Configuration
{
	public abstract class BaseVirtualField : Field
	{
        public override int FieldId => -1;

		public override int GetHashCode()
		{
			return FieldName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
            return obj is BaseVirtualField baseVirtualField
                && baseVirtualField.FieldName == FieldName;
		}
	}
}
