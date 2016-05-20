using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	internal class UserProviderFake : IUserProvider
	{
		public int UserId { get; set; }

		public int GetUserId()
		{
			return UserId;
		}

	    public string GetUserName()
	    {
	        return "Fake user from UserProviderFake";
	    }
	}
}
