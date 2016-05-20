using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.Models.Entities
{
    public static class ArticleExtensions
    {
        public static bool IsPublished(this Article article) {
            return article.IsPublished && !(article.GetChildren().Any(x => !x.IsPublished()));
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
