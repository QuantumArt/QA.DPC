using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.ContentProviders;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Polly.Registry;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public class JsonElasticConfiguration : ElasticConfiguration
    {
        private readonly IConfigurationRoot _config;

        public JsonElasticConfiguration(
            IConfigurationRoot config,
            ILoggerFactory loggerFactory,
            IHttpClientFactory factory,
            PolicyRegistry registry,
            DataOptions dataOptions) : base (factory, registry, loggerFactory, dataOptions)
        {
            _config = config;
        }

        protected override IEnumerable<HighloadApiMethod> GetHighloadApiMethods()
        {
            return new[] {"GetById", "GetByType", "Search"}.Select(n => new HighloadApiMethod() { Title = n}).ToArray();
        }

        public override void SetCachePrefix(string prefix)
        {
        }

        protected override IEnumerable<HighloadApiUser> GetHighloadApiUsers()
        {
            return _config.GetSection("Users").GetChildren().Select(n => new HighloadApiUser {Name = n.Key, Token = n.Value});
        }

        protected override IEnumerable<HighloadApiLimit> GetHighloadApiLimits()
        {
            var methods = GetHighloadApiMethods();
            var section = _config.GetSection("RateLimits");
            foreach (var method in methods)
            {
                var limits = section.GetSection(method.Title).GetChildren();
                foreach (var l in limits)
                {
                    yield return new HighloadApiLimit()
                    {
                        Limit = int.Parse(l["Limit"]),
                        Seconds = int.Parse(l["Seconds"]),
                        Method = method.Title,
                        User = l.Key
                    };
                }
            }
        }

    }
}