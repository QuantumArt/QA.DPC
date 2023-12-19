using System;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Core.API.DI
{
    public class ProductStoreFactory : ProductStoreFactoryBase
    {
        public ProductStoreFactory(
            Func<string, IProductStore> versionFactory,
            ElasticConfiguration configuration,
            ICacheProvider cache,
            DataOptions options)
            : base(versionFactory, configuration, cache, options)
        {
        }

        protected override string MapVersion(SearchEngine engine)
        {
            if (engine.Name.Equals("opensearch", StringComparison.OrdinalIgnoreCase))
            {
                if (engine.Version.StartsWith("2."))
                    return "os2.*";
            }

            if (engine.Name.Equals("elasticsearch", StringComparison.OrdinalIgnoreCase))
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
