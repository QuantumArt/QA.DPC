using System.Collections.Generic;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public abstract class FilterBase : IArticleFilter
    {
        #region IArticleFilter Members

        public virtual IEnumerable<Article> Filter(IEnumerable<Article> items)
        {
            foreach (var item in items)
            {
                if (Matches(item))
                    yield return item;
            }
        }

        public bool Matches(Article item)
        {
            if (item == null)
                return false;

            return OnMatch(item);
        }

        protected abstract bool OnMatch(Article item);

        #endregion
    }
}
