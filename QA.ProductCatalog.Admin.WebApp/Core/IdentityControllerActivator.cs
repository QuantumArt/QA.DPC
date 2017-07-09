﻿using Microsoft.Practices.Unity;
using QA.Core.DPC.QP.Models;
using QA.Core.Web;
using System;
using System.Web.Mvc;
using System.Web.Routing;
using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
    public class IdentityControllerActivator : IControllerActivator
    {
        private const string CustomerCodeKey = "customerCode";

        private readonly IControllerActivator _activator;
        private IUnityContainer _container;

        public IdentityControllerActivator(IUnityContainer container)
        {
            _container = container;
            _activator = new UnityControllerActivator(container);
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            var customerCode = requestContext.HttpContext.Request[CustomerCodeKey];

            if (string.IsNullOrEmpty(customerCode))
            {
                customerCode = requestContext.HttpContext.Session[CustomerCodeKey] as string;
            }
            else
            {
                requestContext.HttpContext.Session[CustomerCodeKey] = customerCode;
            }

            _container.Resolve<IIdentityProvider>().Identity = new Identity(customerCode);
            return _activator.Create(requestContext, controllerType);
        }
    }
}