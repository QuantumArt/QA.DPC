using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.Extensions
{
    public static class ArticleExstensions
    {
        public static IEnumerable<Article> GetAllArticles(this Article article, bool allArticles = false)
        {
            return new[] { article }.GetAllArticles(allArticles);
        }

        public static IEnumerable<Article> GetAllArticles(this IEnumerable<Article> articles, bool allArticles = false)
        {
            return GetAllArticlesInternal(articles, false, allArticles);
        }

        private static IEnumerable<Article> GetAllArticlesInternal(IEnumerable<Article> articles, bool isExsteinsion, bool allArticles)
        {
            foreach (var article in articles)
            {
                if (article != null && (allArticles || article.PublishingMode == PublishingMode.Publish))
                {
                    if (allArticles || !isExsteinsion)
                    {
                        yield return article;
                    }

                    var referencedArticles = new List<Article>();

                    foreach (var field in article.Fields.Values.OfType<SingleArticleField>())
                    {
                        if (field.Item != null)
                        {
                            referencedArticles.AddRange(GetAllArticlesInternal(new[] { field.Item }, field is ExtensionArticleField, allArticles));
                        }
                    }

                    foreach (var field in article.Fields.Values.OfType<MultiArticleField>())
                    {
                        referencedArticles.AddRange(GetAllArticlesInternal(field, false, allArticles));
                    }

                    foreach (var referencedArticle in referencedArticles)
                    {
                        if (referencedArticle != null)
                        {
                            yield return referencedArticle;
                        }
                    }
                }
            }
        }
    }
}
