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
            return base.Equals(obj)
                && obj is VirtualMultiEntityField otherVirtualField
                && otherVirtualField.Path == Path;
		}
	}
}
