using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Options;
using System;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public abstract class ProductStoreFactoryBase : IProductStoreFactory
    {
        private Func<string, IProductStore> _versionFactory;
        private ElasticConfiguration _configuration;
        private IMemoryCache _cache;
        private MemoryCacheEntryOptions _cacheOptions;

        public ProductStoreFactoryBase(Func<string, IProductStore> versionFactory, ElasticConfiguration configuration, IMemoryCache cache, DataOptions options)
        {
            _versionFactory = versionFactory;
            _configuration = configuration;
            _cache = cache;
            _cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(options.VersionCaceExpiration);
        }

        public IProductStore GetProductStore(string language, string state)
        {
            var key = GetKey(language, state);

            var serviceVersion = _cache.GetOrCreate(key, c =>
            {
                c.SetOptions(_cacheOptions);
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
            return $"productstore_{language}_{state}";
        }

        protected abstract string MapVersion(string serviceVersion);    
    }
}
