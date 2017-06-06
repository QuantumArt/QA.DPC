using System.Security.Principal;
using QA.Core.DPC.QP.Models;
using Microsoft.AspNetCore.Http;
using QA.Core.DPC.QP.Services;

namespace QA.DPC.Core.Helpers
{
    public class CoreIdentityProvider : IIdentityProvider
    {
        private readonly IHttpContextAccessor _accessor;

        public CoreIdentityProvider(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Identity Identity
        {
            get => (Identity)_accessor.HttpContext.User.Identity;

            set => _accessor.HttpContext.User = new GenericPrincipal(value, new string[0]);
        }
    }
}
