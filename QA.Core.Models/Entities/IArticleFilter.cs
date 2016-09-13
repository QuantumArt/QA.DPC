using System.Collections.Generic;

namespace QA.Core.Models.Entities
{
    public interface IArticleFilter
    {
        IEnumerable<Article> Filter(IEnumerable<Article> items);

        bool Matches(Article item);
    }
}
