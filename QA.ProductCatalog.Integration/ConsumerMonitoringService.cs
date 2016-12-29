using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.DAL;

namespace QA.ProductCatalog.Integration
{
    public class ConsumerMonitoringService : IConsumerMonitoringService
    {
        private readonly string _connectionString;
        public ConsumerMonitoringService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int[] FindMissingProducts(int[] productIDs)
        {
            var products = new MonitoringRepository(_connectionString)
                .GetByIds(productIDs);
            
            return productIDs
                .Except(products.Select(x => x.Id))
                .ToArray();
        }

        public ProductInfo GetProductInfo(int productId)
        {
            var products = new MonitoringRepository(_connectionString).GetByIds(productId);

            if (products == null || products.Length == 0)
                return null;

            return products[0];
        }

		public void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive)
        {
            var monitoringRepository = new MonitoringRepository(_connectionString);

			monitoringRepository.InsertOrUpdateProductRelevanceStatus(productId, productRelevance, isLive);
        }

        public int[] FindExistingProducts(int[] productIDs)
        {
            var products = new MonitoringRepository(_connectionString)
               .GetByIds(productIDs);

            return products.Select(x => x.Id)                
                .ToArray();
        }

	    public string GetProduct(int id)
	    {
		    return new MonitoringRepository(_connectionString).GetProductXml(id);
	    }
    }
}
