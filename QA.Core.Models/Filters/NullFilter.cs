using System.Collections.Generic;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public class NullFilter:IArticleFilter
    {
        #region IArticleFilter Members

        public IEnumerable<Article> Filter(IEnumerable<Article> items)
        {
            return items;
        }

        public bool Matches(Article item)
        {
            return true;
        }

        #endregion
    }
}
