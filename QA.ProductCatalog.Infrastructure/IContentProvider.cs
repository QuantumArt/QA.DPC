
namespace QA.ProductCatalog.Infrastructure
{
	public interface IContentProvider<TModel>
		where TModel : class
	{
		TModel[] GetArticles();
	}
}
