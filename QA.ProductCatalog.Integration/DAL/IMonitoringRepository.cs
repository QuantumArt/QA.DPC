using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Integration.DAL
{
    public interface IMonitoringRepository
    {
        ProductInfo[] GetByIds(int[] productIDs);

        string GetProductXml(int id);

        void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive);
    }
}