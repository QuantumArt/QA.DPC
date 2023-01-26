using Microsoft.AspNetCore.Http;
using QA.Core.Models.Entities;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Interfaces
{
    public interface ITmfService
    {
        string TmfIdFieldName { get; }

        TmfProcessResult GetProducts(string slug, string version, IQueryCollection query, out ArticleList products);

        TmfProcessResult GetProductById(string slug, string version, string id, out Article product);

        TmfProcessResult DeleteProductById(string slug, string version, string id);

        TmfProcessResult UpdateProductById(string slug, string version, string id, Article product, out Article updatedProduct);

        TmfProcessResult CreateProduct(string slug, string version, Article product, out Article createdProduct);
    }
}
