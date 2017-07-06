using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.QP.API.Services
{
    class TarantoolProductAPIService : IProductSimpleAPIService
    {
        public Article GetProduct(int productId, int definitionId, bool isLive = false)
        {
            return new Article { Id = productId };
        }
    }
}
