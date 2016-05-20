namespace QA.Core.DPC.Integration
{
	public interface IProductSerializer
	{
		ProductInfo Deserialize(string data);
	}
}
