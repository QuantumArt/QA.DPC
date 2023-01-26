using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API.Models;

namespace QA.ProductCatalog.Infrastructure
{
    public interface IProductUpdateService
    {
        InsertData[] Create(Article product, ProductDefinition definition, bool isLive = false, bool createVersions = false);
        void Delete(int productId, ProductDefinition definition);
        InsertData[] Update(Article product, ProductDefinition definition, bool isLive = false, bool createVersions = false);
    }
}