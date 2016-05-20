using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using Quantumart.QPublishing.Database;
using System;

namespace QA.Core.DPC.Loader.Services
{
    public class DBConnectorProxy : IDBConnector
    {
        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly string _connectionString;
        public DBConnectorProxy(string connectionString, IVersionedCacheProvider cacheProvider)
        {
            _connectionString = connectionString;
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
