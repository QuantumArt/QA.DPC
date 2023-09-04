using System;
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
    public class CoreIdentityProviderBase : IIdentityProvider
    {
        protected readonly string _fixedCustomerCode;

        private Identity _identity;

        public CoreIdentityProviderBase(string code = "")
        {
            _fixedCustomerCode = code;
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

        protected virtual Identity GetValue()
        {
            return _identity; 
        }

        protected virtual void SetValue(Identity identity)
        {
            _identity = identity;
        }

        protected virtual string GetCode()
        {
            return _fixedCustomerCode;
        }

        public virtual bool IsFixed => !string.IsNullOrEmpty(_fixedCustomerCode);
    }

}
