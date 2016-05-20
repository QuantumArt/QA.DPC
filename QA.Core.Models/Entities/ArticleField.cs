using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Entities
{
	[Serializable]
    [DebuggerDisplay("{FieldId} {FieldName}")]
    public abstract class ArticleField : IModelObject
    {
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, object> CustomProperties { get; set; } 

        public int? ContentId { get; set; }
        //public string ContentName { get; set; }
        public int? FieldId { get; set; }

        /// <summary>
        /// Системное имя (англ)
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Название на русском
        /// </summary>
        public string FieldDisplayName { get; set; }
    }

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

        #region IEnumerable<Article> Members

        IEnumerator<Article> IEnumerable<Article>.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Items.Values.GetEnumerator();
        }

        #endregion

        public IEnumerable<Article> GetArticles(IArticleFilter filter)
        {
            return filter == null ? Items.Values : filter.Filter(Items.Values);
        }
    }

	[Serializable]
    public class SingleArticleField : ArticleField, IGetArticleField, IGetArticle
    {
       [DefaultValue(null)]
        public Article Item { get; set; }

        public int? SubContentId { get; set; }

        public ArticleField GetField(string name)
        {
            if (Item == null)
                return null;

            return Item.GetField(name);
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

	[Serializable]
    public class PlainArticleField : ArticleField, IGetFieldStringValue
    {
       [DefaultValue(null)]
        public string Value { get; set; }

        [DefaultValue(null)]
		public object NativeValue { get; set; }

        public PlainFieldType PlainFieldType { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }

	[Serializable]
    public class ExtensionArticleField : SingleArticleField, IGetFieldStringValue
    {
        /// <summary>
        /// Значение поля
        /// </summary>
        /// 
        public string Value { get; set; }
        
    }

	[Serializable]
    public class BackwardArticleField : MultiArticleField
    {
        [DefaultValue(null)]
        public string RelationGroupName { get; set; }
    }
}
