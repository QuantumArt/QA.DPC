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
        private readonly string _fixedCustomerCode;

        private readonly string CustomerCodeKey = "customerCode";

        public CoreIdentityProvider(IHttpContextAccessor accessor, string fixedCustomerCode)
        {
            _context = accessor.HttpContext;
            _fixedCustomerCode = fixedCustomerCode;
        }

        public Identity Identity
    {
        get
        {
            var identity = _context.User.Identity as Identity;
            if (identity == null)
            {
                var code = _fixedCustomerCode;
                code = (!string.IsNullOrEmpty(code)) ? code : _context.Request.Query[CustomerCodeKey].FirstOrDefault();
                code = (!string.IsNullOrEmpty(code)) ? code : _context.GetRouteValue(CustomerCodeKey) as string;
                if (code == null)
                {
                    throw new ApplicationException("Customer code is not defined");
                }
                identity = new Identity(code);
                _context.User = new GenericPrincipal(identity, new string[0]);
            }
             return identity;
        }

        set => _context.User = new GenericPrincipal(value, new string[0]);
    }
    }
}
