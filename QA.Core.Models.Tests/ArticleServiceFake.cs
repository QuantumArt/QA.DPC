using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.Models;
using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.Models.Tests
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

		public void LoadStructureCache()
		{
			StructureCacheIsLoaded = true;
		}

	    public Article Read(int articleId, bool excludeArchive = true)
		{
			Article article;
			Articles.TryGetValue(articleId, out article);
			return article;
		}

        public Article Read(int articleId, int contentId, bool excludeArchive = true)
        {
            Article article;
            Articles.TryGetValue(articleId, out article);
            return article;
        }

        public IEnumerable<int> Ids(int contentId, int[] ids, bool excludeArchive = true, string filter = "")
        {
            foreach (int id in ids)
            {
                if (Articles.TryGetValue(id, out Article article) && article.ContentId == contentId)
                {
                    yield return id;
                }
            }
        }

        public IEnumerable<Article> List(int contentId, int[] ids, bool excludeArchive = true, string filter = "")
		{
			foreach (int id in ids)
			{
				if (Articles.TryGetValue(id, out Article article) && article.ContentId == contentId)
				{
					yield return article;
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

		public string GetLinkedItems(int linkId, int id, bool excludeArchive = true)
		{
			return string.Empty;
		}

		public string GetRelatedItems(int fieldId, int? id, bool excludeArchive = true)
		{
			return string.Empty;
		}

	    public Dictionary<int, Dictionary<int, List<int>>> GetLinkedItems(int[] linkIds, int[] ids, bool excludeArchive = true)
	    {
	        throw new NotImplementedException();
	    }

	    public Dictionary<int, Dictionary<int, List<int>>> GetRelatedItems(int[] fieldId, int[] ids, bool excludeArchive = true)
	    {
	        throw new NotImplementedException();
	    }

	    public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
		{
			throw new NotImplementedException();
		}

		public bool IsLive { get; set; }


		InsertData[] IArticleService.BatchUpdate(IEnumerable<ArticleData> articles, bool createVersions)
		{
			throw new NotImplementedException();
		}

		InsertData[] IArticleService.BatchUpdate(IEnumerable<Article> articles, bool createVersions)
		{
			throw new NotImplementedException();
		}

		public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
		{
			throw new NotImplementedException();
		}

        public RulesException XamlValidationById(int articleId, bool persistChanges)
        {
            throw new NotImplementedException();
        }
    }
}
