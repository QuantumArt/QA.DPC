﻿using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.Core.ProductCatalog.Actions.Services
{
    public class IdentityUserProvider : IUserProvider
    {
        private readonly IIdentityProvider _identityProvider;

        public IdentityUserProvider(IIdentityProvider identityProvider)
        {
            _identityProvider = identityProvider;
        }

        public int GetUserId()
        {
            return _identityProvider.Identity.UserId;
        }

        public string GetUserName()
        {
            throw new NotImplementedException();
        }
    }
}