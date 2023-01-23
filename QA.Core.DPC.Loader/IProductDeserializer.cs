using QA.Core.Models.Entities;
using Article = QA.Core.Models.Entities.Article;

namespace QA.Core.DPC.Loader
{
    public interface IProductDeserializer
    {
        Article Deserialize(IProductDataSource productDataSource, Models.Configuration.Content definition);
    }
}
