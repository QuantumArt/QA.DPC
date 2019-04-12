﻿using System;
using System.Linq;
using System.Security.Principal;
 using System.Threading;
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

        protected bool _useSession;
        
        protected string _fixedCustomerCode;

        private static AsyncLocal<Identity> _threadStorage;


        public CoreIdentityProvider(IHttpContextAccessor accessor)
        {
            _context = accessor.HttpContext;
            
            if (_context == null && _threadStorage == null)
            {
                _threadStorage = new AsyncLocal<Identity>();
            }
        }

        public Identity Identity
        {
            get
            {
                Identity identity = GetValue();
                
                if (identity == null)
                {
                    identity = new Identity(GetCode());
                    SetValue(identity);
                }
                
                return identity;
            }

            set
            {
                SetValue(value);
            } 
        }

        private Identity GetValue()
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

        private void SetValue(Identity identity)
        {
            if (_context != null)
            {
                _context.User = new GenericPrincipal(identity, new string[0]);
    
                if (_useSession)
                {
                    _context.Session.SetString(CustomerCodeKey, Identity.CustomerCode);
                }
            }
            else
            {
                _threadStorage.Value = identity;
            }          
        }

        private string GetCode()
        {
            var code = _fixedCustomerCode;
            if (!string.IsNullOrEmpty(code)) return code;

            if (_context != null)
            {
                code = _context.Request.Query[CustomerCodeKey].FirstOrDefault();
                if (!string.IsNullOrEmpty(code)) return code;

                code = _context.GetRouteValue(CustomerCodeKey) as string;
                if (!string.IsNullOrEmpty(code)) return code;
            
                if (_useSession)
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
    
    public class CoreIdentityFixedProvider : CoreIdentityProvider
    {
        public CoreIdentityFixedProvider(IHttpContextAccessor accessor, string fixedCustomerCode) : base(accessor)
        {
            _fixedCustomerCode = fixedCustomerCode;
        }
    }
}
