using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public interface IServiceFactory
	{
		FieldService GetFieldService();
		ArticleService GetArticleService();
		ContentService GetContentService();
	}
}