using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Configuration;

namespace QA.Core.Models.Entities
{
    [Serializable]
    [DebuggerDisplay("{Id} {ContentName}({ContentId})")]
    public class Article : IModelObject, IGetArticleField, IEnumerable<ArticleField>
    {
        public int ContentId { get; set; }
        public string ContentName { get; set; }
        public string ContentDisplayName { get; set; }
        public PublishingMode PublishingMode { get; set; }
        #region Системные поля QP
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Status { get; set; }
        public bool IsPublished { get; set; }

        [DefaultValue(false)]
        public bool Splitted { get; set; }
        public bool Visible { get; set; }
        [DefaultValue(false)]
        public bool Archived { get; set; }
        public int Id { get; set; }

        [DefaultValue(false)]
        public bool HasVirtualFields { get; set; }

        #endregion

        public Dictionary<string, ArticleField> Fields { get; set; }

        public Article() : this(0) { }

        public Article(int id)
        {
            Fields = new Dictionary<string, ArticleField>();
            Id = id;
        }


        public ArticleField GetField(string name)
        {
            if (name == null)
                return null;

            ArticleField field = null;
            if (Fields != null && Fields.TryGetValue(name, out field))
            {
                return field;
            }
            return null;
        }

        #region IEnumerable<ArticleField> Members

        IEnumerator<ArticleField> IEnumerable<ArticleField>.GetEnumerator()
        {
            return Fields.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Fields.Values.GetEnumerator();
        }

        #endregion
    }
}
