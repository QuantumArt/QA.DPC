using QA.Core.DPC.QP.Models;
using System.Security.Principal;
using System.Threading;

namespace QA.Core.DPC.QP.Servives
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
