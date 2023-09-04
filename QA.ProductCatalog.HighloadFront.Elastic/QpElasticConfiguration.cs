using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Polly.Registry;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class QpElasticConfiguration : ElasticConfiguration
    {
        private readonly IContentProvider<ElasticIndex> _indexProvider;
        private readonly IContentProvider<HighloadApiUser> _userProvider;
        private readonly IContentProvider<HighloadApiLimit> _limitProvider;
        private readonly IContentProvider<HighloadApiMethod> _methodProvider;
        
        private readonly ICacheProvider _cacheProvider;
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(60);
        private IConnectionProvider _connectionProvider;
        private string _cachePrefix;

        
        public QpElasticConfiguration(
            IContentProvider<ElasticIndex> indexProvider,
            IContentProvider<HighloadApiUser> userProvider,
            IContentProvider<HighloadApiLimit> limitProvider,
            IContentProvider<HighloadApiMethod> methodProvider,
            ICacheProvider cacheProvider,
            IHttpClientFactory factory,
            PolicyRegistry registry,
            ILoggerFactory loggerFactory,
            DataOptions dataOptions,
            IConnectionProvider connectionProvider
        ) : base (factory, registry, loggerFactory, dataOptions)
        {
            _indexProvider = indexProvider;
            _userProvider = userProvider;
            _limitProvider = limitProvider;
            _methodProvider = methodProvider;
            _cacheProvider = cacheProvider;
            _connectionProvider = connectionProvider;

        }

        public string GetCacheKey(string key)
        {
            var cachePrefix = _cachePrefix ?? _connectionProvider.GetCustomer().CustomerCode;
            return cachePrefix + "_" + key;
        }

        public override void SetCachePrefix(string prefix)
        {
            _cachePrefix = prefix;
        }

        public override IEnumerable<ElasticIndex> GetElasticIndices()
        {
            return _cacheProvider.GetOrAdd(GetCacheKey("ElasticIndexes"), _indexProvider.GetTags(), _cacheTimeSpan, _indexProvider.GetArticles);
        }

        protected override IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            return _cacheProvider.GetOrAdd(GetCacheKey("HighloadApiUsers"), _userProvider.GetTags(), _cacheTimeSpan, _userProvider.GetArticles);
        }

        protected override IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            return _cacheProvider.GetOrAdd(GetCacheKey("HighloadApiLimits"), _limitProvider.GetTags(), _cacheTimeSpan, _limitProvider.GetArticles);
        }
        
        protected override IEnumerable<HighloadApiMethod> GetHighloadApiMethods()
        {
            return _cacheProvider.GetOrAdd(GetCacheKey("HighloadApiMethods"), _methodProvider.GetTags(), _cacheTimeSpan, _methodProvider.GetArticles);
        }

        public override IndexOperationSyncer GetSyncer(string language, string state)
        {
            return _cacheProvider.GetOrAdd(GetCacheKey("ElasticSyncers"), _indexProvider.GetTags(), _cacheTimeSpan, GetSyncerMap)[GetElasticKey(language, state)];
        }
    }
}