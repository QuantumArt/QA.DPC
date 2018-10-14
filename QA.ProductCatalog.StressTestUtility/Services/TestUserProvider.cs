using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.StressTestUtility.Services
{
	public class TestUserProvider : IUserProvider
	{
		public int GetUserId()
		{
			return Configuration.UserId;
		}

		public string GetUserName()
		{
			return "StressTest";
		}
	}
}
