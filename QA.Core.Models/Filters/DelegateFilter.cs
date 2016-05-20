using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Filters
{
    public class DelegateFilter : FilterBase
    {
        private readonly Func<Article, bool> _match;

        public DelegateFilter(Func<Article, bool> match)
        {
            Throws.IfArgumentNull(match, _ => match);

            _match = match;
        }
        protected override bool OnMatch(Article item)
        {
            return _match(item);
        }

        public static implicit operator DelegateFilter(Func<Article, bool> match)
        {
            return new DelegateFilter(match);
        }
    }
}
