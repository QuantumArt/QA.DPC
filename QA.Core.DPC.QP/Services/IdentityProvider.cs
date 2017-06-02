using System.Security.Principal;
using System.Threading;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public class IdentityProvider : IIdentityProvider
    {
        public Identity Identity
        {
            get
            {
                return Thread.CurrentPrincipal.Identity as Identity;
            }

            set
            {
                Thread.CurrentPrincipal = new GenericPrincipal(value, new string[0]);
            }
        }
    }
}
