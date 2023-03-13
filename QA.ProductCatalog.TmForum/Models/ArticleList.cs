using QA.Core.Models.Entities;

namespace QA.ProductCatalog.TmForum.Models
{
    public class ArticleList
    {
        public int TotalCount { get; set; }
        public List<Article> Articles { get; set; }

        public bool IsPartial { get { return TotalCount != Articles.Count; } }

        public ArticleList(int size)
        {
            Articles = new(size);
        }
    }
}
