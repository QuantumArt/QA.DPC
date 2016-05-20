using System;
namespace QA.Core.Models.Entities
{
    public interface IGetArticleField
    {
        ArticleField GetField(string name);
    }
}
