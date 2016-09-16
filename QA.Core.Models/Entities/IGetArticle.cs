namespace QA.Core.Models.Entities
{
    public interface IGetArticle : IModelObject
    {
        Article GetItem(IArticleFilter filter);
    }
}
