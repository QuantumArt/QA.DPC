﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Nest;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic.Extensions;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class QpElasticConfiguration : IElasticConfiguration
    {
        private readonly IContentProvider<ElasticIndex> _indexProvider;
        private readonly IContentProvider<HighloadApiUser> _userProvider;
        private readonly IContentProvider<HighloadApiLimit> _limitProvider;
        private readonly IVersionedCacheProvider2 _cacheProvider;
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);
        private readonly int _timeout = 5;
        private readonly ILogger _logger;
        private readonly DataOptions _options;


        public QpElasticConfiguration(
            IContentProvider<ElasticIndex> indexProvider,
            IContentProvider<HighloadApiUser> userProvider,
            IContentProvider<HighloadApiLimit> limitProvider,
            IVersionedCacheProvider2 cacheProvider,
            ILogger logger,
            IOptions<DataOptions> options
        )
        {
            _indexProvider = indexProvider;
            _userProvider = userProvider;
            _limitProvider = limitProvider;
            _cacheProvider = cacheProvider;
            _logger = logger;
            _options = options.Value;
        }

        public IEnumerable<ElasticIndex> GetElasticIndices()
        {
            return _cacheProvider.GetOrAdd("ElasticIndexes", _indexProvider.GetTags(), _cacheTimeSpan, _indexProvider.GetArticles);
        }

        public int GetElasticTimeout()
        {
            return _options.ElasticTimeout != 0 ? _options.ElasticTimeout : _timeout;
        }

        protected IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            return _cacheProvider.GetOrAdd("HighloadApiUsers", _userProvider.GetTags(), _cacheTimeSpan, _userProvider.GetArticles);
        }

        protected IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            return _cacheProvider.GetOrAdd("HighloadApiLimits", _limitProvider.GetTags(), _cacheTimeSpan, _limitProvider.GetArticles);
        }

        public Dictionary<string, IElasticClient> GetClientMap()
        {
            var elasticIndices = GetElasticIndices().ToArray();
            var a = elasticIndices.ToDictionary(
                index => GetElasticKey(index.Language, index.State),
                index => GetElasticClient(index.Name, index.Url, _logger, index.DoTrace, GetElasticTimeout())
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
            return _cacheProvider.GetOrAdd("ElasticClients", _indexProvider.GetTags(), _cacheTimeSpan, GetClientMap)[GetElasticKey(language, state)];
        }

        public IndexOperationSyncer GetSyncer(string language, string state)
        {
            return _cacheProvider.GetOrAdd("ElasticSyncers", _indexProvider.GetTags(), _cacheTimeSpan, GetSyncerMap)[GetElasticKey(language, state)];
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

        public static IElasticClient GetElasticClient(string index, string address, ILogger logger, bool doTrace, int timeout)
        {
            var connectionPool = GetConnectionPool(address);

            var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                .DefaultIndex(index)
                .RequestTimeout(TimeSpan.FromSeconds(timeout))
                .DisableDirectStreaming()
                .EnableTrace(msg => logger.Log(() => msg, EventLevel.Trace), doTrace)
                .ThrowExceptions();

            return new ElasticClient(settings);
        }

        private static IConnectionPool GetConnectionPool(string address)
        {
            IConnectionPool connectionPool;
            if (address.Contains(";"))
            {
                var uris = address.Split(';').Select(n => new Uri(n.Trim())).ToArray();
                connectionPool = new StaticConnectionPool(uris);
            }
            else
            {
                var node = new Uri(address);
                connectionPool = new SingleNodeConnectionPool(node);
            }
            return connectionPool;
        }
    }
}