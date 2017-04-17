using System.Security.Principal;

namespace QA.Core.DPC.QP.Models
{
    public class Identity : IIdentity
    {
        public string AuthenticationType { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string Name { get; private set; }
        public string CustomerCode { get; private set; }
        public int UserId { get; private set; }

        public Identity()
        {
            IsAuthenticated = false;
            UserId = 0;
        }

        public Identity(string customerCode)
            : this()
        {
            CustomerCode = customerCode;
        }
    }
}
