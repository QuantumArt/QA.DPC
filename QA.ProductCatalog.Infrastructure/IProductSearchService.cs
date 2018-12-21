using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IProductSearchService
	{
		int[] SearchProducts(string slug, string version, string query);
        int[] ExtendedSearchProducts(string slug, string version, JToken query);

    }
}
