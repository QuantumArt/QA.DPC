using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public class IdentityUserProvider : IUserProvider
    {
        private readonly IIdentityProvider _identityProvider;

        public IdentityUserProvider(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
        }

        public int GetUserId()
        {
            return _identityProvider.Identity?.UserId ?? 1;
        }

        public string GetUserName()
        {
            return _identityProvider.Identity?.Name ?? "admin";
        }
    }
}
