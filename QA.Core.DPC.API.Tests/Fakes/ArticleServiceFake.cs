using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.DPC.API.Test.Fakes
{
	//public class ArticleServiceFake : IArticleService
	//{
	//	public ArticleData[] Articles { get; private set; }

	//	#region IArticleService implementation
	//	public Article Save(Article article)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public Article New(int contentId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public MessageResult Delete(int articleId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public void SimpleDelete(int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public MessageResult Publish(int contentId, int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public void SimplePublish(int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public MessageResult SetArchiveFlag(int contentId, int[] articleIds, bool flag)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public void SimpleSetArchiveFlag(int[] articleIds, bool flag)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public void BatchUpdate(IEnumerable<ArticleData> articles)
	//	{
	//		Articles = articles.ToArray();
	//	}

	//	public void LoadStructureCache()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public Article Read(int articleId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public IEnumerable<Article> List(int contentId, int[] ids)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public string GetLinkedItems(int linkId, int id)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public string GetRelatedItems(int fieldId, int? id)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public string[] GetFieldValues(int[] ids, int contentId, string fieldName)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public bool IsLive { get; set; }

	//	public QPConnectionScope CreateQpConnectionScope()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	InsertData[] IArticleService.BatchUpdate(IEnumerable<ArticleData> articles)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public InsertData[] BatchUpdate(IEnumerable<Article> articles)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public Dictionary<int, bool> CheckRelationSecurity(int contentId, int[] ids, bool isDeletable)
	//	{
	//		throw new NotImplementedException();
	//	}
	//	#endregion

	//	Article IArticleService.Save(Article article)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	Article IArticleService.New(int contentId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	MessageResult IArticleService.Delete(int articleId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	void IArticleService.SimpleDelete(int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	MessageResult IArticleService.Publish(int contentId, int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	void IArticleService.SimplePublish(int[] articleIds)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	MessageResult IArticleService.SetArchiveFlag(int contentId, int[] articleIds, bool flag)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	void IArticleService.SimpleSetArchiveFlag(int[] articleIds, bool flag)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	InsertData[] IArticleService.BatchUpdate(IEnumerable<Article> articles)
	//	{
	//		throw new NotImplementedException();
	//	}
	
	//	void IReadOnlyArticleService.LoadStructureCache()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	Article IReadOnlyArticleService.Read(int articleId)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	IEnumerable<Article> IReadOnlyArticleService.List(int contentId, int[] ids)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	string IReadOnlyArticleService.GetLinkedItems(int linkId, int id)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	string IReadOnlyArticleService.GetRelatedItems(int fieldId, int? id)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	string[] IReadOnlyArticleService.GetFieldValues(int[] ids, int contentId, string fieldName)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	QPConnectionScope IQPService.CreateQpConnectionScope()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}
}
