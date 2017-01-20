using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Collections.Generic;
using QA.Core.DPC.Loader.Services;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
	public class ArticleServiceProfiler : ProfilerBase, IArticleService
	{
		private readonly IArticleService _articleService;

		public ArticleServiceProfiler(IArticleService articleService, ILogger logger)
			: base(logger)
		{
			if (articleService == null)
				throw new ArgumentNullException("articleService");

			_articleService = articleService;
			Service = _articleService.GetType().Name;
		}

		#region IArticleService implementation
		public QPConnectionScope CreateQpConnectionScope()
		{
			// TODO: Implement this method
			throw new NotImplementedException();
		}

		public void LoadStructureCache()
		{
			var token = CallMethod();
			_articleService.LoadStructureCache();
			EndMethod(token);
		}

		public Article Read(int articleId)
		{
			var token = CallMethod("Read", "articleId = {0}", articleId);
			var result = _articleService.Read(articleId);
			EndMethod(token, result);
			return result;
		}


        public Article Read(int articleId, int contentId)
        {
            var token = CallMethod("Read", "articleId = {0}, contentId = {1}", articleId, contentId);
            var result = _articleService.Read(articleId, contentId);
            EndMethod(token, result);
            return result;
        }


        public IEnumerable<Article> List(int contentId, int[] ids)
		{
			var token = CallMethod("List", "contentId = {0}, ids = {1}", contentId, string.Join(",", ids));
			var result = _articleService.List(contentId, ids);
			EndMethod(token, "IEnumerable<Article>");
			return result;
		}

		public Article Save(Article article)
		{
			var token = CallMethod("Save", "articleId = {0}, ContentId = {1}", article.Id, article.ContentId);
			var result = _articleService.Save(article);
			EndMethod(token, result);
			return result;
		}

		public Article New(int contentId)
		{
			var token = CallMethod("New", "contentId = {0}", contentId);
			var result = _articleService.New(contentId);
			EndMethod(token, result);
			return result;
		}

		public MessageResult Delete(int articleId)
		{
			var token = CallMethod("Delete", "articleId = {0}", articleId);
			var result = _articleService.Delete(articleId);
			EndMethod(token, result);
			return result;
		}

		public void SimpleDelete(int[] articleIds)
		{
			var token = CallMethod("SimpleDelete", "articleIds = {0}", string.Join(",", articleIds));
			_articleService.SimpleDelete(articleIds);
			EndMethod(token);
		}

		public MessageResult Publish(int contentId, int[] articleIds)
		{
			var token = CallMethod("Publish", "contentId = {0}, articleIds = {1}", contentId, string.Join(",", articleIds));
			var result = _articleService.Publish(contentId, articleIds);
			EndMethod(token, result);
			return result;
		}

		public void SimplePublish(int[] articleIds)
		{
			var token = CallMethod("SimplePublish", "articleIds = {0}", string.Join(",", articleIds));
			_articleService.SimplePublish(articleIds);
			EndMethod(token);
		}

		public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
		{
			var token = CallMethod("SetArchiveFlag", "contentId = {0}, articleIds = {1}, flag = {2}", contentId, string.Join(",", articleIds), flag);
			var result = _articleService.SetArchiveFlag(contentId, articleIds, flag);
			EndMethod(token, result);
			return result;
		}

		public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
		{
			var token = CallMethod("SimpleSetArchiveFlag", "articleIds = {1}, flag = {2}", string.Join(",", articleIds), flag);
			_articleService.SimpleSetArchiveFlag(articleIds, flag);
			EndMethod(token);
		}

		public string GetLinkedItems(int linkId, int id)
		{
			var token = CallMethod("GetLinkedItems", "linkId = {0}, id = {1}", linkId, id);
			var result = _articleService.GetLinkedItems(linkId, id);
			EndMethod(token, result);
			return result;
		}

		public string GetRelatedItems(int fieldId, int? id)
		{
			var token = CallMethod("GetRelatedItems", "fieldId = {0}, id = {1}", fieldId, id);
			var result = _articleService.GetRelatedItems(fieldId, id);
			EndMethod(token, result);
			return result;
		}

		public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
		{
			throw new NotImplementedException();
		}

		public bool IsLive
		{
			get { return _articleService.IsLive; }
			set { _articleService.IsLive = value; }
		}

		#endregion

		#region Private methods
		private void EndMethod(ProfilerToken token, MessageResult result)
		{
			if (result == null)
			{
				EndMethod(token, "");
			}
			else
			{
				EndMethod(token, "Type = {0}, Text = {1}, FailedIds = {2}", result.Type, result.Text, result.FailedIds == null ? "" : string.Join(",", result.FailedIds));
			}
		}

		private void EndMethod(ProfilerToken token, Article result)
		{
			EndMethod(token, "Id = {0}, ContentId = {1}", result.Id, result.ContentId);
		}		
		#endregion

		public InsertData[] BatchUpdate(IEnumerable<Quantumart.QP8.BLL.Services.API.Models.ArticleData> articles)
		{
			return null;
		}


		public InsertData[] BatchUpdate(IEnumerable<Article> articles)
		{
			return null;
		}

		public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
		{
			throw new NotImplementedException();
		}

        public RulesException XamlValidationById(int articleId)
        {
            return null;
        }


    }
}
