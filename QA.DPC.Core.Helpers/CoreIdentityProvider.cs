﻿using System;
using System.Linq;
using System.Security.Principal;
 using System.Threading;
 using QA.Core.DPC.QP.Models;
using Microsoft.AspNetCore.Http;
 using Microsoft.AspNetCore.Http.Features;
 using Microsoft.AspNetCore.Routing;
using QA.Core.DPC.QP.Services;

namespace QA.DPC.Core.Helpers
{
    public class CoreIdentityProvider : CoreIdentityProviderBase
    {
        private readonly HttpContext _context;

        private readonly string CustomerCodeKey = "customerCode";

        protected bool _useSession;
        
        private static AsyncLocal<Identity> _threadStorage;


        public CoreIdentityProvider(IHttpContextAccessor accessor, string customerCode = null) : base(customerCode)
        {
            _context = accessor.HttpContext;
            
            if (_context == null && _threadStorage == null)
            {
                _threadStorage = new AsyncLocal<Identity>();
            }
        }


        protected override Identity GetValue()
        {
            Identity identity;
            
            if (_context != null)
            {
                identity = _context.User.Identity as Identity;
            }
            else
            {
                identity = _threadStorage.Value;
            }

            return identity;
        }

        protected override void SetValue(Identity identity)
        {
            if (_context != null)
            {
                _context.User = new GenericPrincipal(identity, new string[0]);
    
                if (_useSession && _context.Features.Get<ISessionFeature>()?.Session != null)
                {
                    _context.Session.SetString(CustomerCodeKey, Identity.CustomerCode);
                }
            }
            else
            {
                _threadStorage.Value = identity;
            }          
        }

        protected override string GetCode()
        {
            var code = _fixedCustomerCode;
            if (!string.IsNullOrEmpty(code)) return code;

            if (_context != null)
            {
                if (_context.Request.HasFormContentType && _context.Request.Form.ContainsKey(CustomerCodeKey))
                {
                    code = _context.Request.Form[CustomerCodeKey].ToString();
                }
                if (!string.IsNullOrEmpty(code)) return code;
                
                code = _context.Request.Query[CustomerCodeKey].FirstOrDefault();
                if (!string.IsNullOrEmpty(code)) return code;

                code = _context.GetRouteValue(CustomerCodeKey) as string;
                if (!string.IsNullOrEmpty(code)) return code;
            
                if (_useSession && _context.Features.Get<ISessionFeature>()?.Session != null)
                {
                    code = (!string.IsNullOrEmpty(code)) ? code : _context.Session.GetString(CustomerCodeKey);
                    if (!string.IsNullOrEmpty(code)) return code;
                }               
            }
            
            code = SingleCustomerCoreProvider.Key;
            return code;    
        }
    }

    public class CoreIdentityWithSessionProvider : CoreIdentityProvider
    {
        public CoreIdentityWithSessionProvider(IHttpContextAccessor accessor) : base(accessor)
        {
            _useSession = true;
        }
    }
}
