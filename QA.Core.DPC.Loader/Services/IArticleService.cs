using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.Core.DPC.Loader.Services
{
	public interface IArticleService : IReadOnlyArticleService
	{
		Article Save(Article article);
		Article New(int contentId);
		MessageResult Delete(int articleId);
		void SimpleDelete(int[] articleIds);
		MessageResult Publish(int contentId, int[] articleIds);
		void SimplePublish(int[] articleIds);
		MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag);
		void SimpleSetArchiveFlag(int[] articleIds, bool flag);
		InsertData[] BatchUpdate(IEnumerable<ArticleData> articles);
		InsertData[] BatchUpdate(IEnumerable<Article> articles);
		Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable);
	    RulesException XamlValidationById(int articleId, bool persistChanges);
	}
}
