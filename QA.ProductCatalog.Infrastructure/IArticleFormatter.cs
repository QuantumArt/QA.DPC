using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IArticleFormatter : IFormatter<Article>
	{
		string Serialize(Article product, IArticleFilter filter, bool includeRegionTags);
	}
}