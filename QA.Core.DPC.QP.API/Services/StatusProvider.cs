using QA.Core.Cache;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace QA.Core.DPC.QP.API.Services
{
    public class StatusProvider : IStatusProvider
    {
        private const string StatusCacheKey = "db_statuses";
        private const string Query = @"
            select
                cast(s.STATUS_TYPE_ID as int) StatusId,
	            s.STATUS_TYPE_NAME StatusName
            from
                status_type s";

        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly IConnectionProvider _connectionProvider;     

        public StatusProvider(IVersionedCacheProvider cacheProvider, IConnectionProvider connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _connectionProvider = connectionProvider;
        }

        public string GetStatusName(int statusId)
        {
            var dictionary = _cacheProvider.GetOrAdd(
                StatusCacheKey,
                new[] { CacheTags.QP8.Content, CacheTags.QP8.StatusType },
                TimeSpan.FromHours(1),
                () => GetDictionary());

            if (dictionary.TryGetValue(statusId, out string status))
            {
                return status;
            }
            else
            {
                return null;
            }
        }

        private Dictionary<int, string> GetDictionary()
        {
            var connection = _connectionProvider.GetConnection();
            var dbContext = new DBConnector(connection);

            return dbContext.GetRealData(Query)
                    .AsEnumerable()
                    .Select(row => Converter.ToModelFromDataRow<StatusModel>(row))
                    .ToDictionary(s => s.StatusId, s => s.StatusName);
        }    
    }

    internal class StatusModel
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
