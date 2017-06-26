using System.Linq;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;

namespace QA.ProductCatalog.Integration
{
    public class ConsumerMonitoringService : IConsumerMonitoringService
    {
        private readonly IMonitoringRepository _repository;

        public ConsumerMonitoringService(IMonitoringRepository repository)
        {
            _repository = repository;
        }

        public int[] FindMissingProducts(int[] productIDs)
        {
            var products = _repository.GetByIds(productIDs);
            
            return productIDs
                .Except(products.Select(x => x.Id))
                .ToArray();
        }

        public ProductInfo GetProductInfo(int productId)
        {
            var products = _repository.GetByIds(new []{productId});

            if (products == null || products.Length == 0)
                return null;

            return products[0];
        }

		public void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive)
        {
			_repository.InsertOrUpdateProductRelevanceStatus(productId, productRelevance, isLive);
        }

        public int[] FindExistingProducts(int[] productIDs)
        {
            return _repository.GetByIds(productIDs).Select(x => x.Id).ToArray();
        }

	    public string GetProduct(int id)
	    {
		    return _repository.GetProductXml(id);
	    }
    }
}
