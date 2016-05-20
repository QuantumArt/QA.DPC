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
#warning biryukovp: Возможно, надо переделать проверку авторизованности
			var userId = _userProvider.GetUserId(); //TODO: переделать проверку авторизованности
            return userId > 0;
        }
    }
}
