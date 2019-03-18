using QA.ProductCatalog.ContentProviders;

namespace QA.DPC.Core.Helpers
{
    public class QpUserProviderSecurityChecker : ISecurityChecker
    {
        
        private readonly IUserProvider _userProvider;

        public QpUserProviderSecurityChecker (IUserProvider userProvider)
        {
            _userProvider = userProvider;
        }

        public bool CheckAuthorization()
        {
            var userId = _userProvider.GetUserId(); 
            return userId > 0;
        }
    }
}