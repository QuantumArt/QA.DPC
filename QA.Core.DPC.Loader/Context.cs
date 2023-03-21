using System.Collections.Generic;
using Article = QA.Core.Models.Entities.Article;

namespace QA.Core.DPC.Loader
{
    public class Context
    {
        private int _minArticleId;
        private readonly Dictionary<int, List<Article>> _extensionMap;

        public Context()
        {
            _extensionMap = new Dictionary<int, List<Article>>();
            _minArticleId = 0;
        }

        public void AddExtensionArticle(int parentId, Article article)
        {
            if (!_extensionMap.ContainsKey(parentId))
            {
                _extensionMap[parentId] = new List<Article>();
            }

            _extensionMap[parentId].Add(article);
        }

        public void TakeIntoAccount(int id)
        {
            if (id < _minArticleId)
            {
                _minArticleId = id;
            }
        }

        public void UpdateExtensionArticles()
        {
            var id = _minArticleId;
            foreach (var list in _extensionMap.Values)
            {
                id--;

                foreach (var article in list)
                {
                    article.Id = id;
                }
            }
        }
    }
}
