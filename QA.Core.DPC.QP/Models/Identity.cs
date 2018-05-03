using System;
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

        public Identity(int userId = 0, string userName = "", bool isAuthenticated = false)
        {
            UserId = userId;
            Name = userName;
            IsAuthenticated = isAuthenticated;
        }

        public Identity(string customerCode, int userId = 0, string userName = "", bool isAuthenticated = false)
            : this(userId, userName, isAuthenticated)
        {
            CustomerCode = customerCode;
        }       
    }
}
