using Microsoft.Extensions.Logging;
using QA.Core.Cache;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Polly.Registry;


namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class QpElasticConfiguration : ElasticConfiguration
    {
        private readonly IContentProvider<ElasticIndex> _indexProvider;
        private readonly IContentProvider<HighloadApiUser> _userProvider;
        private readonly IContentProvider<HighloadApiLimit> _limitProvider;
        private readonly IContentProvider<HighloadApiMethod> _methodProvider;
       
        private readonly VersionedCacheProviderBase _cacheProvider;
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(60);

        public QpElasticConfiguration(
            IContentProvider<ElasticIndex> indexProvider,
            IContentProvider<HighloadApiUser> userProvider,
            IContentProvider<HighloadApiLimit> limitProvider,
            IContentProvider<HighloadApiMethod> methodProvider,
            VersionedCacheProviderBase cacheProvider,
            IHttpClientFactory factory,
            PolicyRegistry registry,
            ILoggerFactory loggerFactory,
            DataOptions dataOptions
        ) : base (factory, registry, loggerFactory, dataOptions)
        {
            _indexProvider = indexProvider;
            _userProvider = userProvider;
            _limitProvider = limitProvider;
            _methodProvider = methodProvider;
            _cacheProvider = cacheProvider;

        }

        public override IEnumerable<ElasticIndex> GetElasticIndices()
        {
            return _cacheProvider.GetOrAdd("ElasticIndexes", _indexProvider.GetTags(), _cacheTimeSpan, _indexProvider.GetArticles, true, CacheItemPriority.NeverRemove);
        }

        protected override IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            return _cacheProvider.GetOrAdd("HighloadApiUsers", _userProvider.GetTags(), _cacheTimeSpan, _userProvider.GetArticles, true, CacheItemPriority.NeverRemove);
        }

        protected override IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            return _cacheProvider.GetOrAdd("HighloadApiLimits", _limitProvider.GetTags(), _cacheTimeSpan, _limitProvider.GetArticles, true, CacheItemPriority.NeverRemove);
        }
        
        protected override IEnumerable<HighloadApiMethod> GetHighloadApiMethods()
        {
            return _cacheProvider.GetOrAdd("HighloadApiMethods", _methodProvider.GetTags(), _cacheTimeSpan, _methodProvider.GetArticles, true, CacheItemPriority.NeverRemove);
        }

        public override IndexOperationSyncer GetSyncer(string language, string state)
        {
            return _cacheProvider.GetOrAdd("ElasticSyncers", _indexProvider.GetTags(), _cacheTimeSpan, GetSyncerMap, true, CacheItemPriority.NeverRemove)[GetElasticKey(language, state)];
        }

    }
}