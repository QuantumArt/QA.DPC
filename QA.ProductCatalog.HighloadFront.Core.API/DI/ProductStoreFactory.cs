using Microsoft.Extensions.Caching.Memory;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using QA.Core.Cache;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    public class ProductStoreFactory : ProductStoreFactoryBase
    {
        public ProductStoreFactory(Func<string, IProductStore> versionFactory, ElasticConfiguration configuration, VersionedCacheProviderBase cache, DataOptions options)
            : base(versionFactory, configuration, cache, options)
        {
        }

        protected override string MapVersion(SearchEngine engine)
        {
            if (engine.Name.Equals("opensearch", StringComparison.OrdinalIgnoreCase))
            {
                if (engine.Version.StartsWith("2."))
                    return "8.*";
            }

            if (engine.Name.Equals("ElasticSearch", StringComparison.OrdinalIgnoreCase))
            {
                if (engine.Version.StartsWith("5."))
                    return "5.*";
                if (engine.Version.StartsWith("6."))
                    return "6.*";
                if (engine.Version.StartsWith("8."))
                    return "8.*";
            }

            throw ElasticVersionNotSupported(engine.Version);
        }
    }
}
