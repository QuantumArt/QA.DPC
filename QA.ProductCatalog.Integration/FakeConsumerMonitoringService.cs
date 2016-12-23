using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Integration
{
   public class FakeConsumerMonitoringService : IConsumerMonitoringService
    {

        public int[] FindMissingProducts(int[] productIDs)
        {
            return new int[] { };
        }

        public int[] FindExistingProducts(int[] productIDs)
        {
            return new int[] { };
        }

       public ProductInfo GetProductInfo(int productId)
       {
           return null;
       }

	   public void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive)
	   {
	   }

	   public string GetProduct(int id)
	   {
		   throw new NotImplementedException();
	   }
    }
}
