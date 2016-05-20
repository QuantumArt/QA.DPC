using System;
using System.Collections.Generic;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.DPC.Loader
{
	public class ReadOnlyArticleServiceAdapter : IReadOnlyArticleService
	{
		protected readonly string QpConnString;

		protected readonly ArticleService ArticleService;

		protected readonly IContextStorage ContextStorage;

		public ReadOnlyArticleServiceAdapter(ArticleService articleService, string qpConnString, IContextStorage contextStorage)
        {
            if (articleService == null)
                throw new ArgumentNullException("articleService");

            ArticleService = articleService;

			QpConnString = qpConnString;

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

		public virtual Article Read(int articleId)
		{
            return ArticleService.Read(articleId, true);
		}

        public virtual Article Read(int articleId, int contentId)
        {
            return ArticleService.Read(articleId, contentId, true);
        }

        public virtual IEnumerable<Article> List(int contentId, int[] ids)
		{
			return ArticleService.List(contentId, ids);
		}

		public string GetLinkedItems(int linkId, int id)
		{
			return ArticleService.GetLinkedItems(linkId, id);
		}

		public string GetRelatedItems(int fieldId, int? id)
		{
			return ArticleService.GetRelatedItems(fieldId, id);
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
