using System.Linq;
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

		}

		public Task<int[]> SendProductsAsync(Models.Entities.Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
		{
			return Task.FromResult(products.Select(n => n.Id).ToArray());
		}

		public int[] SendProducts(Models.Entities.Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
		{
			return products.Select(n => n.Id).ToArray();
		}

		public Task DeleteProductsAsync(Models.Entities.Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
		{
			return Task.Delay(1);
		}

		public void DeleteProducts(Models.Entities.Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
		{
		}

		public Task UnpublishProductsAsync(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			return Task.Delay(1);
		}

		public void UnpublishProducts(Models.Entities.Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
		}
        #endregion	
	}
}
