using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;


namespace QA.Core.DPC.QP.Cache
{
    public class CustomerQP8CacheItemWatcher : CustomerCacheItemWatcher
    {
        private readonly string _cmdText = @"SELECT [CONTENT_ID], [LIVE_MODIFIED], [STAGE_MODIFIED] FROM [CONTENT_MODIFICATION] WITH (NOLOCK)";

        public CustomerQP8CacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator, IConnectionProvider connectionProvider, ILogger logger)
            : base(mode, invalidator, connectionProvider, logger)
        {
        }

        public CustomerQP8CacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator, IConnectionProvider connectionProvider, ILogger logger, TimeSpan pollPeriod, int dueTime)
            : base(mode, pollPeriod, invalidator, connectionProvider, logger, dueTime, true)
        {
        }

        protected override void GetData(Dictionary<int, ContentModification> newValues)
        {

            using (var conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(_cmdText, conn))
                {
                    cmd.CommandType = CommandType.Text;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }
                    // производим запрос - без этого не будет работать dependency
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new ContentModification
                            {
                                ContentId = Convert.ToInt32(reader["CONTENT_ID"]),
                                LiveModified = Convert.ToDateTime(reader["LIVE_MODIFIED"]),
                                StageModified = Convert.ToDateTime(reader["STAGE_MODIFIED"])
                            };

                            newValues[item.ContentId] = item;
                        }
                    }
                }

            }
        }
    }
}
