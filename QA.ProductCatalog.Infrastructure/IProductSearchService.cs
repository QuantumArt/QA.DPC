namespace QA.ProductCatalog.Infrastructure
{
	public interface IProductSearchService
	{
		int[] SearchProducts(string slug, string version, string query);
	}
}
