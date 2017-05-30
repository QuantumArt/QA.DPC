using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.Data;
using Quantumart.QP8.BLL;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.Loader.Services
{
	public class StructureCacheTracker : CacheItemTracker
	{
		private readonly string _qpConnectionString;

		public StructureCacheTracker(IConnectionProvider connectionProvider)
		{
			_qpConnectionString = connectionProvider.GetConnection();
		}

		protected override void OnTrackChanges(List<TableModification> changes)
		{
			using (var cs = new QPConnectionScope(_qpConnectionString))
			{
				var con = cs.DbConnection;

				if (con.State != ConnectionState.Open)
					con.Open();

				foreach (string tableNameAsTag in CacheTags.QP8.All)
				{
					var cmd = new SqlCommand("select MAX(MODIFIED) from " + tableNameAsTag, con);

					var maxModifiedObj = cmd.ExecuteScalar();

					DateTime? maxModified = maxModifiedObj is DBNull ? null : (DateTime?)maxModifiedObj;

					if (maxModified.HasValue)
						changes.Add(new TableModification
						{
							TableName = tableNameAsTag,
							LiveModified = maxModified.Value,
							StageModified = maxModified.Value
						});
				}
			}
		}
	}
}
