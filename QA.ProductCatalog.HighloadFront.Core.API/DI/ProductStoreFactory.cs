using Microsoft.Extensions.Caching.Memory;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using QA.Core.Cache;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    public class ProductStoreFactory : ProductStoreFactoryBase
    {
        public ProductStoreFactory(Func<string, IProductStore> versionFactory, ElasticConfiguration configuration, VersionedCacheProviderBase cache, DataOptions options)
            : base(versionFactory, configuration, cache, options)
        {    
        }

        protected override string MapVersion(string serviceVersion)
        {
            if (serviceVersion.StartsWith("5."))
                return "5.*";
            if (serviceVersion.StartsWith("6."))
                return "6.*";
            if (serviceVersion.StartsWith("8."))
                return "8.*";
            throw ElasticVersionNotSupported(serviceVersion);
        }
    }
}
