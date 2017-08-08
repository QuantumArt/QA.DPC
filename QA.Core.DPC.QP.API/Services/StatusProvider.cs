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
        private const string StatusCacheKey = "statuses";
        private const string Query = @"
            select
                cast(c.CONTENT_ID as int) ContentId,
                cast(s.STATUS_TYPE_ID as int) StatusId,
	            s.STATUS_TYPE_NAME StatusName
            from
                content c
                join status_type s on c.SITE_ID = s.SITE_ID";

        private readonly IVersionedCacheProvider _cacheProvider;
        private readonly IConnectionProvider _connectionProvider;     

        public StatusProvider(IVersionedCacheProvider cacheProvider, IConnectionProvider connectionProvider)
        {
            _cacheProvider = cacheProvider;
            _connectionProvider = connectionProvider;
        }

        public string GetStatusName(int contentId, int statusId)
        {
            var key = GetKey(contentId, statusId);

            var dictionary = _cacheProvider.GetOrAdd(
                StatusCacheKey,
                new[] { CacheTags.QP8.Content, CacheTags.QP8.StatusType },
                TimeSpan.FromHours(1),
                () => GetDictionary());

            if (dictionary.TryGetValue(key, out string status))
            {
                return status;
            }
            else
            {
                return null;
            }
        }

        private Dictionary<string, string> GetDictionary()
        {
            var connection = _connectionProvider.GetConnection();
            var dbContext = new DBConnector(connection);

            return dbContext.GetRealData(Query)
                    .AsEnumerable()
                    .Select(row => Converter.ToModelFromDataRow<StatusModel>(row))
                    .ToDictionary(s => GetKey(s.ContentId, s.StatusId), s => s.StatusName);
        }

        private string GetKey(int contentId, int statusId)
        {
            return $"{contentId}_{statusId}";
        }
    }

    internal class StatusModel
    {
        public int ContentId { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }
}
