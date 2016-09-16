using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.Entities
{
    public static class ArticleExtensions
    {
        public static bool IsPublished(this Article article)
        {
            return article.IsPublished && !(GetChildren(article).Any(x => !x.IsPublished()));
        }

        public static IEnumerable<Article> GetChildren(this Article article)
        {
            return article
                .OfType<SingleArticleField>()
                .Select(x => x.Item)
                .Concat(article
                .OfType<MultiArticleField>()
                .SelectMany(x => x));
        }
    }
}
