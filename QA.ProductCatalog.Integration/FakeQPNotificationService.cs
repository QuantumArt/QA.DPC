using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Integration
{
	public class FakeQPNotificationService : IQPNotificationService
	{
        public async Task<int[]> SendProductsAsync(Article[] products, bool isStage, string userName, int userId, bool localize, string[] forcedСhannels = null)
        {
            await Task.Delay(100);
            return new int[0];
        }

        public int[] SendProducts(Article[] products, bool isStage, string userName, int userId, bool localize, string[] forcedСhannels = null)
        {
            return new int[0];
        }     

		public Task DeleteProductsAsync(int[] ids, string userName, int userId, string[] forcedСhannels = null)
	    {
			return Task.Delay(100);
	    }

		public void DeleteProducts(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
		}

		public Task UnpublishProductsAsync(int[] ids, string userName, int userId, string[] forcedСhannels = null)
	    {
			return Task.Delay(100);
	    }

		public void UnpublishProducts(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
		}

		public void DeleteProducts(string[] data, int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
		}

		public Task<int[]> SendProductsAsync(QA.Core.Models.Entities.Article[] products, bool isStage, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}

		public int[] SendProducts(QA.Core.Models.Entities.Article[] products, bool isStage, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}

		public Task DeleteProductsAsync(QA.Core.Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}

		public void DeleteProducts(QA.Core.Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}

		public Task UnpublishProductsAsync(QA.Core.Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}

		public void UnpublishProducts(QA.Core.Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new NotImplementedException();
		}
	}
}
