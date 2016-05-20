using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QA.Core.Models.Entities
{
    public interface IGetArticle : IModelObject
    {
        Article GetItem(IArticleFilter filter);
    }
}
