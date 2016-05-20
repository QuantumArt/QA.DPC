using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class ArticleServiceFake : IArticleService
	{
		public Dictionary<int, Article> Articles { get; private set; }
		public bool StructureCacheIsLoaded { get; private set; }

		public ArticleServiceFake()
		{
			Articles = new Dictionary<int, Article>();
			StructureCacheIsLoaded = false;
		}

		public QPConnectionScope CreateQpConnectionScope()
		{
			// TODO: Implement this method
			throw new NotImplementedException();
		}

		public event EventHandler<ArticleEventArgs> ArticleSaved;

		public void LoadStructureCache()
		{
			StructureCacheIsLoaded = true;
		}

		public Article Read(int articleId, bool isLive = true)
		{
			throw new NotImplementedException();
		}

		public Article Read(int articleId)
		{
			Article article;
			Articles.TryGetValue(articleId, out article);
			return article;
		}

        public Article Read(int articleId, int contentId)
        {
            Article article;
            Articles.TryGetValue(articleId, out article);
            return article;
        }

        public IEnumerable<Article> List(int contentId, int[] ids)
		{
			foreach (int id in ids)
			{
				Article article;

				if (Articles.TryGetValue(id, out article))
				{
					if (article.ContentId == contentId)
					{
						yield return article;
					}
				}
			}
		}

		public Article Save(Article article)
		{
			if (article.Id == 0)
			{
				article.Id = Articles.Keys.DefaultIfEmpty(0).Max() + 1;
			}

			Articles[article.Id] = article;

			if (ArticleSaved != null)
			{
				ArticleSaved(this, new ArticleEventArgs(article));
			}

			return article;
		}

		public Article New(int contentId)
		{
			var article = (Article)Activator.CreateInstance(typeof(Article), true);
			article.ContentId = contentId;
			article.FieldValues = new List<FieldValue>();
			return article;
		}

		public MessageResult Delete(int articleId)
		{
			Articles.Remove(articleId);
			return MessageResult.Confirm("");
		}

		public void SimpleDelete(int[] articleIds)
		{
			foreach (int articleId in articleIds)
			{
				Articles.Remove(articleId);
			}
		}

		public MessageResult Publish(int contentId, int[] articleIds)
		{
			return MessageResult.Confirm("");
		}

		public void SimplePublish(int[] articleIds)
		{
		}

		public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
		{
			foreach (int articleId in articleIds)
			{
				Articles[articleId].Archived = flag;
			}

			return MessageResult.Confirm("");
		}

		public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
		{
			foreach (int articleId in articleIds)
			{
				Articles[articleId].Archived = flag;
			}
		}

		public string GetLinkedItems(int linkId, int id)
		{
			return string.Empty;
		}

		public string GetRelatedItems(int fieldId, int? id)
		{
			return string.Empty;
		}

		public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
		{
			throw new NotImplementedException();
		}

		public bool IsLive { get; set; }


		InsertData[] IArticleService.BatchUpdate(IEnumerable<ArticleData> articles)
		{
			throw new NotImplementedException();
		}

		InsertData[] IArticleService.BatchUpdate(IEnumerable<Article> articles)
		{
			throw new NotImplementedException();
		}

		public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
		{
			throw new NotImplementedException();
		}
	}
}
