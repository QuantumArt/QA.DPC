using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Entities
{
    public interface IGetArticles : IModelObject
    {
        IEnumerable<Article> GetArticles(IArticleFilter filter);
    }
}
