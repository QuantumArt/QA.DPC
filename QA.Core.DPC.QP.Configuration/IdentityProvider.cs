using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Http;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.QP.Configuration
{
    public class IdentityProvider : IIdentityProvider
    {

        private readonly HttpContext _httpContext;
        public IdentityProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContext = httpContextAccessor.HttpContext;
        }
        
        public IdentityProvider(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }
        
        public Identity Identity
        {
            get
            {
                var identity = Thread.CurrentPrincipal?.Identity as Identity;

                if (identity == null && _httpContext != null)
                {
                    identity = _httpContext.User?.Identity as Identity;
                }

                return identity;
            }

            set
            {
                var principal = new GenericPrincipal(value, new string[0]);

                if (_httpContext != null)
                {
                    
                    _httpContext.User = principal;
                }

                Thread.CurrentPrincipal = principal;
            }
        }

        public bool IsFixed => false;
    }
}
