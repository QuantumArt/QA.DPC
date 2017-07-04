using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using Quantumart.QPublishing.Database;
using System;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Loader.Services
{
    public class DBConnectorProxy : IDBConnector
    {
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly string _connectionString;
        public DBConnectorProxy(IConnectionProvider connectionProvider, IVersionedCacheProvider cacheProvider)
        {
            _connectionString = connectionProvider.GetConnection();
            _cacheProvider = cacheProvider;
        }
        public string GetUrlForFileAttribute(int fieldId)
        {
            return _cacheProvider.GetOrAdd($"DBConnectorProxy.GetUrlForFileAttribute_{fieldId}",
                CacheTags.Merge(CacheTags.QP8.Site, CacheTags.QP8.Field),
                TimeSpan.FromMinutes(10),
                () =>
                {
                    return GetConnector().GetUrlForFileAttribute(fieldId);
                });
        }

        private DBConnector GetConnector()
        {
            return new DBConnector(_connectionString);
        }
    }
}
