using Microsoft.AspNetCore.Http;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.TmForum.Interfaces
{
    public interface ITmfService
    {
        string TmfIdFieldName { get; }

        List<Article> GetProducts(string slug, string version, DateTime lastUpdateDate, IQueryCollection query);

        Article GetProductById(string slug, string version, string id);

        bool TryDeleteProductById(string slug, string version, string id);

        Article TryUpdateProductById(string slug, string version, string id, Article product);

        Article TryCreateProduct(string slug, string version, Article product);
    }
}
