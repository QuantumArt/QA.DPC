using System.Collections.Generic;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Services
{
    public interface IReadOnlyArticleService : IQPService
	{
		void LoadStructureCache();
		Article Read(int articleId, bool excludeArchive = true);
        Article Read(int articleId, int contentId, bool excludeArchive = true);
        IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = true);
		string GetLinkedItems(int linkId, int id, bool excludeArchive = true);
		string GetRelatedItems(int fieldId, int? id, bool excludeArchive = true);
		string[] GetFieldValues(int[] ids, int contentId, string fieldName);

		bool IsLive { get; set; }
	}
}
