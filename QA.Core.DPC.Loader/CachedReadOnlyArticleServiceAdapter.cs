using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;

namespace QA.Core.DPC.Loader
{
	/// <summary>
	/// кеширует статьи, полученные методами Read и List
	/// кеш живет пока живет инстанс
	/// не использовать как синглтон!
	/// не thread safe
	/// </summary>
	public class CachedReadOnlyArticleServiceAdapter : ReadOnlyArticleServiceAdapter
	{
		public CachedReadOnlyArticleServiceAdapter(ArticleService articleService, IConnectionProvider connectionProvider, IContextStorage contextStorage, ILogger logger)
			: base(articleService, connectionProvider, contextStorage)
		{
			_logger = logger;
		}

		private readonly Dictionary<int, Article[]> _listFromQpCacheByContentId = new Dictionary<int, Article[]>();

		private readonly Dictionary<int, Article> _loadedArticles = new Dictionary<int, Article>();
		private readonly ILogger _logger;

		public override Article Read(int articleId)
		{
			if (!_loadedArticles.ContainsKey(articleId))
			{
				_loadedArticles[articleId] = base.Read(articleId);

				_logger.Debug("loading article {0} from qp", articleId);
			}
			else
				_logger.Debug("returning article {0} from cache", articleId);

			return _loadedArticles[articleId];
		}

		private Article GetLoadedArticleOrNull(int articleId)
		{
			Article value;
			_loadedArticles.TryGetValue(articleId, out value);
			return value;

		}

		public override IEnumerable<Article> List(int contentId, int[] ids)
		{
			if (ids != null && ids.Length > 0)
			{
				int[] articlesToLoadIds = ids.Except(_loadedArticles.Keys).ToArray();

				if (articlesToLoadIds.Any())
					FillArticlesCache(base.List(contentId, articlesToLoadIds));

				_logger.Debug(() => string.Format("returning list of articles: {0} from cache and {1} from qp", ids.Length - articlesToLoadIds.Length, articlesToLoadIds.Length));

				return ids.Where(x => _loadedArticles.ContainsKey(x)).Select(x => GetLoadedArticleOrNull(x)).Where(x => x != null).ToArray();
			}
			else
			{
				if (!_listFromQpCacheByContentId.ContainsKey(contentId))
				{
					var allArticlesFromContent = base.List(contentId, null).ToArray();

					_listFromQpCacheByContentId[contentId] = allArticlesFromContent;

					FillArticlesCache(allArticlesFromContent);

					_logger.Debug("loading all {0} articles from content {1}", allArticlesFromContent.Length, contentId);
				}
				else
					_logger.Debug("returning all articles from cache from content {0}", contentId);

				return _listFromQpCacheByContentId[contentId];
			}
		}

		private void FillArticlesCache(IEnumerable<Article> allArticlesFromContent)
		{
			foreach (Article article in allArticlesFromContent)
				_loadedArticles[article.Id] = article;
		}
	}
}
