using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using QA.Core;
using QA.Core.Cache;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class QpElasticConfiguration : IElasticConfiguration
    {
        private readonly IContentProvider<ElasticIndex> _indexProvider;
        private readonly IContentProvider<HighloadApiUser> _userProvider;
        private readonly IContentProvider<HighloadApiLimit> _limitProvider;
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);
        private readonly int _timeout = 5;
        private readonly ILogger _logger;


        public QpElasticConfiguration(
            IContentProvider<ElasticIndex> indexProvider,
            IContentProvider<HighloadApiUser> userProvider,
            IContentProvider<HighloadApiLimit> limitProvider,
            IVersionedCacheProvider cacheProvider,
            ILogger logger
        )
        {
            _indexProvider = indexProvider;
            _userProvider = userProvider;
            _limitProvider = limitProvider;
            _cacheProvider = cacheProvider;
            _logger = logger;
        }

        public IEnumerable<ElasticIndex> GetElasticIndices()
        {
            var tags = new[] {SettingsTitles.ELASTIC_INDEXES_CONTENT_ID.ToString()};
            return _cacheProvider.GetOrAdd("ElasticIndexes", tags, _cacheTimeSpan, _indexProvider.GetArticles);
        }

        protected IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            var tags = new[] {SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID.ToString()};
            return _cacheProvider.GetOrAdd("HighloadApiUsers", tags, _cacheTimeSpan, _userProvider.GetArticles);
        }

        protected IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            var tags = new[] {SettingsTitles.HIGHLOAD_API_LIMITS_CONTENT_ID.ToString()};
            return _cacheProvider.GetOrAdd("HighloadApiLimits", tags, _cacheTimeSpan, _limitProvider.GetArticles);
        }

        public Dictionary<string, IElasticClient> GetClientMap()
        {
            var elasticIndices = GetElasticIndices().ToArray();
            var a = elasticIndices.ToDictionary(
                index => GetElasticKey(index.Language, index.State),
                index => GetElasticClient(index.Name, index.Url, _timeout, _logger, index.DoTrace)
            );
            var defaultIndex = elasticIndices.FirstOrDefault(n => n.IsDefault);
            if (defaultIndex != null)
            {
                var defaultKey = GetElasticKey(null, null);
                var actualKey = GetElasticKey(defaultIndex.Language, defaultIndex.State);
                a[defaultKey] = a[actualKey];
            }
            return a;
        }

        public Dictionary<string, IndexOperationSyncer> GetSyncerMap()
        {
            return GetElasticIndices().ToDictionary(
                index => GetElasticKey(index.Language, index.State),
                index => new IndexOperationSyncer()
            );
        }


        public IElasticClient GetElasticClient(string language, string state)
        {
            var tags = new[] { SettingsTitles.ELASTIC_INDEXES_CONTENT_ID.ToString() };
            return _cacheProvider.GetOrAdd("ElasticClients", tags, _cacheTimeSpan, GetClientMap)[GetElasticKey(language, state)];
        }

        public IndexOperationSyncer GetSyncer(string language, string state)
        {
            var tags = new[] { SettingsTitles.ELASTIC_INDEXES_CONTENT_ID.ToString() };
            return _cacheProvider.GetOrAdd("ElasticSyncers", tags, _cacheTimeSpan, GetSyncerMap)[GetElasticKey(language, state)];
        }

        public string GetReindexUrl(string language, string state)
        {
            return GetElasticIndices().FirstOrDefault(n => n.Language == language && n.State == state)?.ReindexUrl;
        }

        public string GetUserName(string token)
        {
            return GetHighloadApiUsers().FirstOrDefault(n => n.Token == token)?.Name;
        }

        public string GetUserToken(string name)
        {
            return GetHighloadApiUsers().FirstOrDefault(n => n.Name == name)?.Token;
        }

        public RateLimit GetLimit(string name, string profile)
        {
            return GetHighloadApiLimits()
                .Where(n => n.User == name && n.Method == profile)
                .Select(n => new RateLimit() { Limit = n.Limit, Seconds = n.Seconds})
                .FirstOrDefault() ?? new RateLimit() { Limit = 0, Seconds = 1 };
        }


        private string GetElasticKey(string language, string state)
        {
            return (language == null && state == null) ? "default" : $"{language}-{state}";
        }

        private IElasticClient GetElasticClient(string index, string address, int timeout, ILogger logger, bool doTrace)
        {
            var node = new Uri(address);

            var connectionPool = new SingleNodeConnectionPool(node);

            var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                .DefaultIndex(index)
                .RequestTimeout(TimeSpan.FromSeconds(timeout))
                .DisableDirectStreaming()
                .EnableTrace(msg => logger.Log(() => msg, EventLevel.Trace), doTrace)
                .ThrowExceptions();

            return new ElasticClient(settings);
        }
    }
}