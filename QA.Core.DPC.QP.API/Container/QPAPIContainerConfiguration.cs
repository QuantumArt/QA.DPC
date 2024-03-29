﻿using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.API.Services;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Unity;
using Unity.Extension;
using IStatusProvider = QA.ProductCatalog.ContentProviders.IStatusProvider;


namespace QA.Core.DPC.QP.API.Container
{
    public class QPAPIContainerConfiguration : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<IProductSimpleAPIService, TarantoolProductAPIService>();
            Container.RegisterType<IProductSimpleService<JToken, JToken>, TarantoolJsonService>();
            Container.RegisterType<IStatusProvider, StatusProvider>();
        }
    }
}