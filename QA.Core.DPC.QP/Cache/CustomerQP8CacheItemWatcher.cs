using Quantumart.QP8.BLL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.QP.Servives
{
    public class CustomerQP8CacheItemWatcher : CustomerCacheItemWatcher
    {
        private readonly string _cmdText = @"SELECT [CONTENT_ID], [LIVE_MODIFIED], [STAGE_MODIFIED] FROM [CONTENT_MODIFICATION] WITH (NOLOCK)";

        public CustomerQP8CacheItemWatcher(InvalidationMode mode, IContentInvalidator invalidator, IConnectionProvider connectionProvider)
            : base(mode, invalidator, connectionProvider)
        {

        }

        protected override void GetData(Dictionary<int, ContentModification> newValues)
        {
            using (var cs = new QPConnectionScope(_connectionString))
            {
                var con = cs.DbConnection;

                using (SqlCommand cmd = new SqlCommand(_cmdText, con))
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
