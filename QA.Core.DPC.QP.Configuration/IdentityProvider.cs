using QA.Core.DPC.QP.Models;
using System.Security.Principal;
using System.Threading;
using System.Web;

namespace QA.Core.DPC.QP.Services
{
    public class IdentityProvider : IIdentityProvider
    {
        public Identity Identity
        {
            get
            {
                var identity = Thread.CurrentPrincipal.Identity as Identity;
#if !NETSTANDARD
                if (identity == null && HttpContext.Current != null)
                {
                    identity = HttpContext.Current.User?.Identity as Identity;
                }
#endif                

                return identity;
            }

            set
            {
                var principal = new GenericPrincipal(value, new string[0]);

#if !NETSTANDARD
                if (HttpContext.Current != null)
                {
                    
                    HttpContext.Current.User = principal;
                }
#endif

                Thread.CurrentPrincipal = principal;
            }
        }
    }
}
