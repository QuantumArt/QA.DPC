using QA.ProductCatalog.Infrastructure;
using QA.Core.Cache;
using Quantumart.QPublishing.Database;
using System;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.Loader.Services
{
    public class DBConnectorProxy : IDBConnector
    {
        private readonly VersionedCacheProviderBase _cacheProvider;
        private readonly Customer _customer;
        public DBConnectorProxy(IConnectionProvider connectionProvider, VersionedCacheProviderBase cacheProvider)
        {
            _customer = connectionProvider.GetCustomer();
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
            return _customer.DbConnector;
        }
    }
}
