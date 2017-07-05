using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using QA.Core;
using QA.ProductCatalog.HighloadFront.Elastic.Extensions;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class JsonElasticConfiguration : IElasticConfiguration
    {
        private readonly int _timeout = 5;
        private readonly IConfigurationRoot _config;
        private readonly ILogger _logger;
        private readonly DataOptions _dataOptions;


        public JsonElasticConfiguration(
            IConfigurationRoot config,
            ILogger logger,
            IOptions<DataOptions> dataOptions)
        {
            _config = config;
            _logger = logger;
            _dataOptions = dataOptions.Value;
        }

        public IEnumerable<ElasticIndex> GetElasticIndices()
        {
            return _dataOptions.Elastic;
        }

        protected IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            return _config.GetSection("Users").GetChildren().Select(n => new HighloadApiUser {Name = n.Key, Token = n.Value});
        }

        protected IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            var methods = new[] {"GetById", "GetByType", "Search"};
            var section = _config.GetSection("RateLimits");
            foreach (var method in methods)
            {
                var limits = section.GetSection(method).GetChildren();
                foreach (var l in limits)
                {
                    yield return new HighloadApiLimit()
                    {
                        Limit = int.Parse(l["Limit"]),
                        Seconds = int.Parse(l["Seconds"]),
                        Method = method,
                        User = l.Key
                    };
                }
            }
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
            return GetClientMap()[GetElasticKey(language, state)];
        }

        public IndexOperationSyncer GetSyncer(string language, string state)
        {
            return GetSyncerMap()[GetElasticKey(language, state)];
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