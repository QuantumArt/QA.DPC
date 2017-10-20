using System.Threading.Tasks;
using QA.Core.Models.Entities;

namespace QA.ProductCatalog.Infrastructure
{
	public interface IQPNotificationService
	{
		Task<int[]> SendProductsAsync(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null);
		int[] SendProducts(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null);

		Task DeleteProductsAsync(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null);
		void DeleteProducts(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null);

		Task UnpublishProductsAsync(Article[] products, string userName, int userId, string[] forcedСhannels = null);
		void UnpublishProducts(Article[] products, string userName, int userId, string[] forcedСhannels = null);
	}
}
