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
        private VersionedCacheProviderBase _cacheProvider;
        private TimeSpan _expiration;

        public ProductStoreFactoryBase(Func<string, IProductStore> versionFactory, ElasticConfiguration configuration, VersionedCacheProviderBase cacheProvider, DataOptions options)
        {
            _versionFactory = versionFactory;
            _configuration = configuration;
            _cacheProvider = cacheProvider;
            _expiration = options.VersionCacheExpiration;
        }

        public IProductStore GetProductStore(string language, string state)
        {
            return _versionFactory(GetProductStoreVersion(language, state));
        }
        
        public string GetProductStoreVersion(string language, string state)
        {
            var key = GetKey(language, state);
            var serviceVersion = _cacheProvider.GetOrAdd(key, _expiration,  () =>
            {
                var client = _configuration.GetElasticClient(language, state);
                var info = client.GetInfo().Result;
                var version = JObject.Parse(info).SelectToken("version.number").Value<string>();
                return version;
            });

            return MapVersion(serviceVersion);

        }

        private string GetKey(string language, string state)
        {
            return $"VersionNumber_{language}_{state}";
        }

        protected abstract string MapVersion(string serviceVersion);

        public NotImplementedException ElasticVersionNotSupported(string serviceVersion)
        {
            return new NotImplementedException($"Elasticsearch version {serviceVersion} is not supported");
        }
    }
}
