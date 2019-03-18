using System;
using System.Linq;
using System.Security.Principal;
using QA.Core.DPC.QP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using QA.Core.DPC.QP.Services;

namespace QA.DPC.Core.Helpers
{
    public class CoreIdentityProvider : IIdentityProvider
    {
        private readonly HttpContext _context;

        private readonly string CustomerCodeKey = "customerCode";

        protected bool useSession;
        
        protected string _fixedCustomerCode;


        public CoreIdentityProvider(IHttpContextAccessor accessor)
        {
            _context = accessor.HttpContext;
        }

        public Identity Identity
        {
            get
            {
                var identity = _context.User.Identity as Identity;
                if (identity == null)
                {
                    var code = GetCode();
                    identity = new Identity(code);
                    _context.User = new GenericPrincipal(identity, new string[0]);
    
                    if (useSession)
                    {
                        _context.Session.SetString(CustomerCodeKey, code);
                    }
                }
                 return identity;
            }
    
            set => _context.User = new GenericPrincipal(value, new string[0]);
        }

        private string GetCode()
        {
            var code = _fixedCustomerCode;
            if (!string.IsNullOrEmpty(code)) return code;

            code = _context.Request.Query[CustomerCodeKey].FirstOrDefault();
            if (!string.IsNullOrEmpty(code)) return code;

            code = _context.GetRouteValue(CustomerCodeKey) as string;
            if (!string.IsNullOrEmpty(code)) return code;
            
            if (useSession)
            {
                code = (!string.IsNullOrEmpty(code)) ? code : _context.Session.GetString(CustomerCodeKey);
                if (!string.IsNullOrEmpty(code)) return code;
            }
            
            code = SingleCustomerCoreProvider.Key;
            return code;    
        }
    }

    public class CoreIdentityWithSessionProvider : CoreIdentityProvider
    {
        public CoreIdentityWithSessionProvider(IHttpContextAccessor accessor) : base(accessor)
        {
            useSession = true;
        }
    }
    
    public class CoreIdentityFixedProvider : CoreIdentityProvider
    {
        public CoreIdentityFixedProvider(IHttpContextAccessor accessor, string fixedCustomerCode) : base(accessor)
        {
            _fixedCustomerCode = fixedCustomerCode;
        }
    }
}
