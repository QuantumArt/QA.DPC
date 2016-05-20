using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.DPC.Loader.Services
{
	public interface IReadOnlyArticleService : IQPService
	{
		void LoadStructureCache();
		Article Read(int articleId);
        Article Read(int articleId, int contentId);
        IEnumerable<Article> List(int contentId, int[] ids);
		string GetLinkedItems(int linkId, int id);
		string GetRelatedItems(int fieldId, int? id);
		string[] GetFieldValues(int[] ids, int contentId, string fieldName);

		bool IsLive { get; set; }
	}
}
