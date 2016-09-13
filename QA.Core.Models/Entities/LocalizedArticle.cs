using System;
using System.Globalization;

namespace QA.Core.Models.Entities
{
    public class LocalizedArticle
    {
        public LocalizedArticle(Article article, CultureInfo culture)
        {
            if (article == null)
            {
                throw new ArgumentNullException(nameof(article));
            }

            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            Article = article;
            Culture = culture;
        }

        public Article Article { get; private set; }

        public CultureInfo Culture { get; private set; }
    }
}
