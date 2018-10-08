using System;
using System.Linq;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;

namespace QA.ProductCatalog.ImpactService.API.Helpers
{
    public class ElasticConfiguration
    {
        
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

        
        public static IElasticClient GetElasticClient(string index, string address, ILogger logger, bool doTrace, int timeout)
        {
            var connectionPool = GetConnectionPool(address);

            var settings = new ConnectionSettings(connectionPool, s => new JsonNetSerializer(s).EnableStreamResponse())
                .DefaultIndex(index)
                .RequestTimeout(TimeSpan.FromSeconds(timeout))
                .DisableDirectStreaming()
                //.EnableTrace(msg => logger.Log(() => msg, EventLevel.Trace), doTrace)
                .ThrowExceptions();

            return new ElasticClient(settings);
        }
    }
}