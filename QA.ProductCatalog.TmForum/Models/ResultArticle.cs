using QA.Core.Models.Entities;

namespace QA.ProductCatalog.TmForum.Models
{
    public class ResultArticle
    {
        public Article Article { get; set; }
        public string[] ValidationErrors { get; set; }
    }
}
