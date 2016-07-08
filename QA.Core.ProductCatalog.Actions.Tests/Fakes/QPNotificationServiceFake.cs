using QA.ProductCatalog.Infrastructure;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	public class QPNotificationServiceFake : IQPNotificationService
	{	
        #region IQPNotificationService Members	
		public Task DeleteProductsAsync(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
			return Task.Delay(1);
		}

		public void DeleteProducts(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{			
		}

		public Task UnpublishProductsAsync(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
			return Task.Delay(1);
		}

		public void UnpublishProducts(int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
		}

		public void DeleteProducts(string[] data, int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public Task<int[]> SendProductsAsync(Models.Entities.Article[] products, bool isStage, string userName, int userId, bool localize, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public int[] SendProducts(Models.Entities.Article[] products, bool isStage, string userName, int userId, bool localize, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public Task DeleteProductsAsync(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public void DeleteProducts(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public Task UnpublishProductsAsync(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}

		public void UnpublishProducts(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			throw new System.NotImplementedException();
		}
        #endregion	
	}
}
