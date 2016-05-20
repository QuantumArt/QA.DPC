using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters.Base
{
    public class AnyFilter: FilterBase
    {
        private readonly IArticleFilter[] _filters;
        public AnyFilter(params IArticleFilter[] filters)
        {
            _filters = filters;
        }

        #region IArticleFilter Members

        protected override bool OnMatch(Article item)
        {
            return _filters.Any(f => f.Matches(item));
        }

        #endregion
    }
}
