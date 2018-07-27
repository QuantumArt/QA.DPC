using System;
using System.Collections.Generic;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.DPC.Loader
{
	public class ReadOnlyArticleServiceAdapter : IReadOnlyArticleService
	{
		protected readonly string QpConnString;

		protected readonly ArticleService ArticleService;

		protected readonly IContextStorage ContextStorage;

		public ReadOnlyArticleServiceAdapter(ArticleService articleService, IConnectionProvider connectionProvider, IContextStorage contextStorage)
        {
            if (articleService == null)
                throw new ArgumentNullException("articleService");

            ArticleService = articleService;

			QpConnString = connectionProvider.GetConnection();

		    ContextStorage = contextStorage;
        }

		public QPConnectionScope CreateQpConnectionScope()
		{
			return new QPConnectionScope(QpConnString);
		}

		public void LoadStructureCache()
		{
			ArticleService.LoadStructureCache(ContextStorage);
		}

		public virtual Article Read(int articleId, bool excludeArchive = true)
		{
            return ArticleService.Read(articleId, true, excludeArchive);
		}

        public virtual Article Read(int articleId, int contentId, bool excludeArchive = true)
        {
            return ArticleService.Read(articleId, contentId, true, excludeArchive);
        }

        public virtual IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = true, string filter = "")
		{
			return ArticleService.List(contentId, ids, excludeArchive, filter);
		}

		public string GetLinkedItems(int linkId, int id, bool excludeArchive = true)
		{
			return ArticleService.GetLinkedItems(linkId, id, excludeArchive);
		}

	    public Dictionary<int, Dictionary<int, List<int>>> GetLinkedItems(int[] linkIds, int[] ids,
	        bool excludeArchive = true)
	    {
	        return ArticleService.GetLinkedItemsMultiple(linkIds, ids, excludeArchive);
	    } 

		public string GetRelatedItems(int fieldId, int? id, bool excludeArchive = true)
		{
			return ArticleService.GetRelatedItems(fieldId, id, excludeArchive);
		}

	    public Dictionary<int, Dictionary<int, List<int>>> GetRelatedItems(int[] linkIds, int[] ids,
	        bool excludeArchive = true)
	    {
	        return ArticleService.GetRelatedItemsMultiple(linkIds, ids, excludeArchive);
	    } 

		public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
		{
			return ArticleService.GetFieldValues(ids, contentId, fieldName);
		}

		public bool IsLive
		{
			get { return ArticleService.IsLive; }

			set { ArticleService.IsLive = value; }
		}
	}
}
