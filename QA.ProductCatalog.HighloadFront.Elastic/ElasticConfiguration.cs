using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Polly.Registry;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.HighloadFront.Options;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public abstract class ElasticConfiguration
    {
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _factory;
        private readonly PolicyRegistry _registry;
        public DataOptions DataOptions { get; }
        
        protected ElasticConfiguration(
            IHttpClientFactory factory,
            PolicyRegistry registry,
            ILoggerFactory loggerFactory,
            DataOptions dataOptions            
            )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _factory = factory;
            _registry = registry;            
            DataOptions = dataOptions;
        }

        public virtual IEnumerable<ElasticIndex> GetElasticIndices()
        {
            return DataOptions.Elastic;
        }
        
        protected  abstract IEnumerable<HighloadApiUser> GetHighloadApiUsers();

        protected  abstract IEnumerable<HighloadApiLimit> GetHighloadApiLimits();
        
        protected abstract IEnumerable<HighloadApiMethod> GetHighloadApiMethods();


        protected string GetElasticKey(string language = null, string state = null)
        {
            return (language == null && state == null) ? "default" : $"{language}-{state}";
        }

        public ElasticIndex GetElasticIndex(string language, string state)
        {
            return GetIndicesMap()[GetElasticKey(language, state)];
        }

        protected string GetElasticKey(ElasticIndex index)
        {
            return GetElasticKey(index.Language, index.State);
        }

        public virtual Dictionary<string, ElasticIndex> GetIndicesMap()
        {
            var elasticIndices = GetElasticIndices().ToArray();
            var a = elasticIndices.ToDictionary(
                index => GetElasticKey(index.Language, index.State),
                index => index
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

        public virtual IndexOperationSyncer GetSyncer(string language, string state)
        {
            return GetSyncerMap()[GetElasticKey(language, state)];
        }

        public ElasticClient GetElasticClient(ElasticIndex index)
        {
            return new ElasticClient(_factory, _registry, index.Name, index.Urls, _logger, DataOptions);
        }

        public virtual string GetUserName(string token)
        {
            return GetHighloadApiUsers().FirstOrDefault(n => n.Token == token)?.Name;
        }

        public virtual string GetUserToken(string name)
        {
            return GetHighloadApiUsers().FirstOrDefault(n => n.Name == name)?.Token;
        }

        public virtual RateLimit GetLimit(string name, string profile)
        {
            return GetHighloadApiLimits()
                       .Where(n => n.User == name && n.Method == profile)
                       .Select(n => new RateLimit() { Limit = n.Limit, Seconds = n.Seconds})
                       .FirstOrDefault() ?? new RateLimit() { Limit = 0, Seconds = 1 };
        }

        public virtual string GetJsonByAlias(string alias)
        {
            var customMethods = GetHighloadApiMethods().Where(n => !n.System);
            var customMethod = customMethods.SingleOrDefault(n =>
                String.Equals(n.Title, alias, StringComparison.InvariantCultureIgnoreCase));
            return customMethod?.Json;
        }
        
        public string GetReindexUrl(string language, string state)
        {
            var index = GetElasticIndices().FirstOrDefault(n => n.Language == language && n.State == state);

            if (index != null)
            {
                if (index.Date == null)
                {
                    return index.ReindexUrl;
                }
                else
                {
                    return $"{index.ReindexUrl}/{index.Date:s}";
                }
            }
            else
            {
                return null;
            }
        }

        public virtual ElasticClient GetElasticClient(string language, string state)
        {
            return GetElasticClient(GetElasticIndex(language, state));
        }
    }
}