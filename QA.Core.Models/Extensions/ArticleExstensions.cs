using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.Models.Extensions
{
    public static class ArticleExstensions
    {
        public static IEnumerable<Article> GetAllArticles(this Article article)
        {
            return new[] { article }.GetAllArticles();
        }

        public static IEnumerable<Article> GetAllArticles(this IEnumerable<Article> articles)
        {
            return GetAllArticles(articles, false);
        }

        private static IEnumerable<Article> GetAllArticles(IEnumerable<Article> articles, bool isExsteinsion)
        {
            foreach (var article in articles)
            {
                if (article != null && article.PublishingMode == PublishingMode.Publish)
                {
                    if (!isExsteinsion)
                    {
                        yield return article;
                    }

                    var referencedArticles = new List<Article>();

                    foreach (var field in article.Fields.Values.OfType<SingleArticleField>())
                    {
                        if (field.Item != null)
                        {
                            referencedArticles.AddRange(GetAllArticles(new[] { field.Item }, field is ExtensionArticleField));
                        }
                    }

                    foreach (var field in article.Fields.Values.OfType<MultiArticleField>())
                    {
                        referencedArticles.AddRange(GetAllArticles(field));
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
