using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Integration
{
	public class FakeQPNotificationService : IQPNotificationService
	{
		public Task SendProductsAsync(string[] data, int[] ids, bool isStage, string userName, int userId, string[] forcedСhannels = null)
	    {
			return Task.Delay(100);
	    }

		public void SendProducts(string[] data, int[] ids, bool isStage, string userName, int userId, string[] forcedСhannels = null)
	    {
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
