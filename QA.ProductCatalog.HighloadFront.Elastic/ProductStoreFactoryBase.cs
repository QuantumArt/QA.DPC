using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using QA.Core.Cache;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public abstract class ProductStoreFactoryBase : IProductStoreFactory
    {
        private Func<string, IProductStore> _versionFactory;
        private ElasticConfiguration _configuration;
        private IVersionedCacheProvider2 _cacheProvider;
        private TimeSpan _expiration;

        public ProductStoreFactoryBase(Func<string, IProductStore> versionFactory, ElasticConfiguration configuration, IVersionedCacheProvider2 cacheProvider, DataOptions options)
        {
            _versionFactory = versionFactory;
            _configuration = configuration;
            _cacheProvider = cacheProvider;
            _expiration = options.VersionCacheExpiration;
        }

        public IProductStore GetProductStore(string language, string state)
        {
            var key = GetKey(language, state);
            var serviceVersion = _cacheProvider.GetOrAdd(key, _expiration,  () =>
            {
                var client = _configuration.GetElasticClient(language, state);
                var info = client.GetInfo().Result;
                var version = JObject.Parse(info).SelectToken("version.number").Value<string>();
                return version;
            });

            var clientVersion = MapVersion(serviceVersion);

            return _versionFactory(clientVersion);
        }

        private string GetKey(string language, string state)
        {
            return $"VersionNumber_{language}_{state}";
        }

        protected abstract string MapVersion(string serviceVersion);    
    }
}
