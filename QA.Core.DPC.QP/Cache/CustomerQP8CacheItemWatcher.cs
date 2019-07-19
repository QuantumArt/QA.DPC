using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;


namespace QA.Core.DPC.QP.Cache
{
    public class CustomerQP8CacheItemWatcher : CustomerCacheItemWatcher
    {
        private string GetCmdText(DatabaseType dbType){
            return $@"SELECT CONTENT_ID, LIVE_MODIFIED, STAGE_MODIFIED FROM CONTENT_MODIFICATION {SqlQuerySyntaxHelper.WithNoLock(dbType)}";
        }

        public CustomerQP8CacheItemWatcher(InvalidationMode mode, 
            IContentInvalidator invalidator, 
            IConnectionProvider connectionProvider, 
            ILogger logger,
            DatabaseType databaseType = DatabaseType.SqlServer)
            : base(mode, invalidator, connectionProvider, logger, databaseType)
        {
        }

        public CustomerQP8CacheItemWatcher(InvalidationMode mode, 
            IContentInvalidator invalidator, 
            IConnectionProvider connectionProvider, 
            ILogger logger, 
            TimeSpan pollPeriod, 
            int dueTime,
            DatabaseType databaseType = DatabaseType.SqlServer)
            : base(mode, pollPeriod, invalidator, connectionProvider, logger, dueTime, true, databaseType: databaseType)
        {
        }

        protected override void GetData(Dictionary<int, ContentModification> newValues)
        {
            DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), DbType);
            DbConnection connection = dbType == DatabaseType.SqlServer
                ? (DbConnection)new SqlConnection(ConnectionString)
                : new NpgsqlConnection(ConnectionString);

            using (connection)
            {
                string query = GetCmdText(dbType);
                
                DbCommand cmd = dbType == DatabaseType.SqlServer
                    ? (DbCommand)new SqlCommand(query)
                    : new NpgsqlCommand(query);
                cmd.Connection = connection;
                using (cmd)
                {
                    cmd.CommandType = CommandType.Text;
                    if (connection.State != ConnectionState.Open)
                    {
                        connection.Open();
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
