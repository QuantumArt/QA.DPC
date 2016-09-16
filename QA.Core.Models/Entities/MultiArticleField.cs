﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace QA.Core.Models.Entities
{
    [Serializable]
    public class MultiArticleField : ArticleField, IEnumerable<Article>, IGetArticles
    {
        public int SubContentId { get; set; }

        public MultiArticleField()
        {
            Items = new Dictionary<int, Article>();
        }

        /// <summary>
        /// key -- идентификатор статьи, value -- статья
        /// </summary>
        public Dictionary<int, Article> Items { get; set; }

        public IEnumerable<Article> GetArticles(IArticleFilter filter)
        {
            return filter == null ? Items.Values : filter.Filter(Items.Values);
        }

        IEnumerator<Article> IEnumerable<Article>.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }
    }
}