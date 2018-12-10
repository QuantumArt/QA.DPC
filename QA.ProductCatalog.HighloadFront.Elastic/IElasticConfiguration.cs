using System.Collections.Generic;
using Nest;
using QA.ProductCatalog.HighloadFront.Options;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Elastic
{
    public interface IElasticConfiguration
    {
        IEnumerable<ElasticIndex> GetElasticIndices();

        IElasticClient GetElasticClient(string language, string state);

        IndexOperationSyncer GetSyncer(string language, string state);

        string GetReindexUrl(string language, string state);

        string GetUserName(string token);

        string GetUserToken(string name);

        RateLimit GetLimit(string name, string profile);

        string GetJsonByAlias(string alias);

        int GetElasticTimeout();
    }
}