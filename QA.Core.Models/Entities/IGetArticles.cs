using System.Collections.Generic;

namespace QA.Core.Models.Entities
{
    public interface IGetArticles : IModelObject
    {
        IEnumerable<Article> GetArticles(IArticleFilter filter);
    }
}
