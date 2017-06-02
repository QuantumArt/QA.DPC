namespace QA.ProductCatalog.Infrastructure
{
   public interface IConsumerMonitoringService
    {
       int[] FindMissingProducts(int[] productIDs);
       int[] FindExistingProducts(int[] productIDs);
       ProductInfo GetProductInfo(int productId);

       void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive);

	   string GetProduct(int id);
    }
}
