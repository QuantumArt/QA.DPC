using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using QA.Core.Logger;
using QA.Core.Cache;
using QA.Core.Data;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;

namespace QA.Core.DPC.Loader
{
    public class QP8CacheItemWatcher : QPCacheItemWatcher
    {
        private readonly string _cmdText = @"SELECT CONTENT_ID, LIVE_MODIFIED, STAGE_MODIFIED FROM CONTENT_MODIFICATION {0}";

        public QP8CacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator, ILogger logger, string connectionName = "qp_database")
            : base(mode, invalidator, logger, connectionName)
        {

        }

        protected override void GetData(Dictionary<int, ContentModification> newValues)
        {
            Enum.TryParse<DatabaseType>(DbType, out var dbType);

            using (var cs = new QPConnectionScope(ConnectionString, dbType))
            {
                var con = cs.DbConnection;
                string query = string.Format(_cmdText, SqlQuerySyntaxHelper.WithNoLock(dbType));
                DbCommand cmd = dbType == DatabaseType.SqlServer 
                    ? (DbCommand)new SqlCommand(query)
                    : new NpgsqlCommand(query);
                    
                using (cmd)
                {
                    cmd.CommandType = CommandType.Text;
                    if (con.State != ConnectionState.Open)
                    {
                        con.Open();
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

            //tsSuppressed.Complete();
        }
    }
}
