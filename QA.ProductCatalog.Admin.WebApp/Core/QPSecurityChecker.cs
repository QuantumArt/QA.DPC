using System.Web;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
    public class QPSecurityChecker : IAdministrationSecurityChecker
    {
		private readonly IUserProvider _userProvider;

		public QPSecurityChecker(IUserProvider userProvider)
		{
			_userProvider = userProvider;
		}

        public bool CheckAuthorization(HttpContextBase context)
        {
			var userId = _userProvider.GetUserId(); 
            return userId > 0;
        }
    }
}
