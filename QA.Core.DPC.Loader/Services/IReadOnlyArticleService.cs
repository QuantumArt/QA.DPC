﻿using System.Collections.Generic;
using Quantumart.QP8.BLL;

namespace QA.Core.DPC.Loader.Services
{
    public interface IReadOnlyArticleService : IQPService
	{
		void LoadStructureCache();
		Article Read(int articleId, bool excludeArchive = true);
        Article Read(int articleId, int contentId, bool excludeArchive = true);
        IEnumerable<int> Ids(int contentId, int[] ids, bool excludeArchive = true, string filter = "");
        IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = true, string filter = "");
		string GetLinkedItems(int linkId, int id, bool excludeArchive = true);
		string GetRelatedItems(int fieldId, int? id, bool excludeArchive = true);
        Dictionary<int, Dictionary<int, List<int>>> GetLinkedItems(int[] linkIds, int[] ids, bool excludeArchive = true);
	    Dictionary<int, Dictionary<int, List<int>>> GetRelatedItems(int[] fieldId, int[] ids, bool excludeArchive = true);


		string[] GetFieldValues(int[] ids, int contentId, string fieldName);

		bool IsLive { get; set; }
	}
}
