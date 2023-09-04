﻿using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.HighloadFront.Interfaces;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public abstract class ProductStoreFactoryBase : IProductStoreFactory
    {
        private Func<string, IProductStore> _versionFactory;
        private ElasticConfiguration _configuration;
        private ICacheProvider _cacheProvider;
        private TimeSpan _expiration;

        public ProductStoreFactoryBase(
            Func<string, IProductStore> versionFactory,
            ElasticConfiguration configuration,
            ICacheProvider cacheProvider,
            DataOptions options)
        {
            _versionFactory = versionFactory;
            _configuration = configuration;
            _cacheProvider = cacheProvider;
            _expiration = options.VersionCacheExpiration;
        }

        public async Task<IProductStore> GetProductStore(string customerCode, string language, string state)
        {
            var store = await GetProductStoreVersion(customerCode, language, state);
            return _versionFactory(store);
        }
        
        public async Task<string> GetProductStoreVersion(string customerCode, string language, string state)
        {
            var key = GetKey(customerCode, language, state);
            var serviceVersion = await _cacheProvider.GetOrAddAsync(
                key,
                Array.Empty<string>(),
                _expiration,
                async () =>
                {
                    var client = _configuration.GetElasticClient(language, state);
                    var info = await client.GetInfo();
                    var searchEngine = new SearchEngine()
                    {
                        Name = JObject.Parse(info).SelectToken("version.distribution")?.Value<string>() ?? "elasticsearch",
                        Version = JObject.Parse(info).SelectToken("version.number")?.Value<string>()
                    };
                    return searchEngine;
                });

            return MapVersion(serviceVersion);
        }

        public NotImplementedException ElasticVersionNotSupported(string serviceVersion)
        {
            return new NotImplementedException($"Search engine version {serviceVersion} is not supported");
        }

        protected abstract string MapVersion(SearchEngine engine);

        private string GetKey(string customerCode, string language, string state)
        {
            return $"VersionNumber_{customerCode}_{language}_{state}";
        }
    }
}
