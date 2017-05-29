namespace QA.Core.DPC.Front
{
	public interface IProductSerializer
	{
		ProductInfo Deserialize(string data);
	}
}
