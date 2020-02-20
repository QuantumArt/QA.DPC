using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Services
{
	public class AlwaysAdminUserProvider : IUserProvider
	{
		public int GetUserId()
		{
			return 1;
		}

	    public string GetUserName()
	    {
	        return "Admin";
	    }

	    public int GetLanguageId()
	    {
		    return 1;
	    }
	}
}
