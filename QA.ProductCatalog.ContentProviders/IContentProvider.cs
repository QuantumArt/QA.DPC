
namespace QA.ProductCatalog.ContentProviders
{
	public interface IContentProvider<TModel>
		where TModel : class
	{
		TModel[] GetArticles();
		
	    string[] GetTags();
	}


}
