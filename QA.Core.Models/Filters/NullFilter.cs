using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
