using System;
using System.Collections.Generic;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.Core.DPC.Loader.Services
{
	public class ArticleServiceAdapter : ReadOnlyArticleServiceAdapter, IArticleService
	{
		public ArticleServiceAdapter(ArticleService articleService, IConnectionProvider connectionProvider, IContextStorage contextStorage)
			: base(articleService, connectionProvider, contextStorage)
		{

		}


		#region IArticleService implementation

		public Article Save(Article article)
		{
			return ArticleService.Save(article);
		}

		public Article New(int contentId)
		{
			return ArticleService.New(contentId);
		}

		public MessageResult Delete(int articleId)
		{
			return ArticleService.Delete(articleId);
		}

		public void SimpleDelete(int[] articleIds)
		{
			ArticleService.SimpleDelete(articleIds);
		}

		public MessageResult Publish(int contentId, int[] articleIds)
		{
			return ArticleService.Publish(contentId, articleIds);
		}

		public void SimplePublish(int[] articleIds)
		{
			ArticleService.SimplePublish(articleIds);
		}

		public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
		{
			return ArticleService.SetArchiveFlag(contentId, articleIds, flag);
		}

		public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
		{
			ArticleService.SimpleSetArchiveFlag(articleIds, flag);
		}

		public InsertData[] BatchUpdate(IEnumerable<ArticleData> articles)
		{
			return ArticleService.BatchUpdate(articles);
		}

		public InsertData[] BatchUpdate(IEnumerable<Article> articles)
		{
			return ArticleService.BatchUpdate(articles);
		}

		public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
		{
			return ArticleService.CheckRelationSecurity(contentId, ids, isDeletable);
		}

        public RulesException XamlValidationById(int articleId)
        {
             return ArticleService.ValidateXamlById(articleId);
        }
        #endregion
    }
}
