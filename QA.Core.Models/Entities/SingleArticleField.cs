using System;
using System.ComponentModel;

namespace QA.Core.Models.Entities
{
    [Serializable]
    public class SingleArticleField : ArticleField, IGetArticleField, IGetArticle
    {
        [DefaultValue(null)]
        public Article Item { get; set; }

        public int? SubContentId { get; set; }

        public ArticleField GetField(string name)
        {
            return Item?.GetField(name);
        }

        public Article GetItem(IArticleFilter filter)
        {
            if (filter == null || filter.Matches(Item))
            {
                return Item;
            }

            return null;
        }

        [DefaultValue(false)]
        public bool Aggregated { get; set; }
    }
}