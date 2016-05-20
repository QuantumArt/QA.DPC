using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Entities
{
    public interface IArticleFilter
    {
        IEnumerable<Article> Filter(IEnumerable<Article> items);
        bool Matches(Article item);
    }
}
