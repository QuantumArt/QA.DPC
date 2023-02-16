using QA.Core.Models.Entities;
using BLL = Quantumart.QP8.BLL;

namespace QA.ProductCatalog.TmForum.Interfaces
{
    public interface ITmfValidatonService
    {
        void ValidateArticle(BLL.RulesException errors, Article article);

        void CheckArticleState(Article article, ref bool result);
    }
}
