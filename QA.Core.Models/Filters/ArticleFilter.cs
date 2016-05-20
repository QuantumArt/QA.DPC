using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;
using QA.Core.Models.Filters;
using QA.Core.Models.Filters.Base;

namespace QA.Core.Models
{
    public static class ArticleFilter
    {
        static Lazy<IArticleFilter> _defaultFilter = new Lazy<IArticleFilter>(() => new AllFilter(
            new NotArchivedFilter(),
            new VisibleFilter()), true);

        static Lazy<IArticleFilter> _liveFilter = new Lazy<IArticleFilter>(() => new AllFilter(
           new NotArchivedFilter(),
           new VisibleFilter(),
           new PublishedFilter()), true);

        public static IArticleFilter DefaultFilter { get { return _defaultFilter.Value; } }
        public static IArticleFilter LiveFilter { get { return _liveFilter.Value; } }
    }
}
