using System.Linq;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Repository.ArticleMatching;
using Quantumart.QP8.BLL.Services.API;


namespace QA.Core.DPC.API.Search
{
	public class ProductSearchService : IProductSearchService
	{
		private readonly IArticleMatchService<ProductQuery> _articleMatchService;
		private readonly IContentDefinitionService _contentDefinitionService;

		public ProductSearchService(IArticleMatchService<ProductQuery> articleMatchService, IContentDefinitionService contentDefinitionService)
		{
			_articleMatchService = articleMatchService;
			_contentDefinitionService = contentDefinitionService;
		}

		public int[] SearchProducts(string slug, string version, string query)
		{
			var definition = _contentDefinitionService.GetServiceDefinition(slug, version);
			var condition = new ProductQuery{ Definition = definition.Content, Query = query, ExstensionContentIds = definition.ExstensionContentIds };
			var products = _articleMatchService.MatchArticles(definition.Content.ContentId, condition, MatchMode.Strict);
			return products.Select(product => product.Id).ToArray();
		}
	}
}