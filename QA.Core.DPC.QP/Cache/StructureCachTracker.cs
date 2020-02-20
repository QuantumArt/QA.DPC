using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QP.ConfigurationService.Models;

namespace QA.Core.DPC.QP.Cache
{
    public class StructureCacheTracker : CacheItemTracker
	{
		private readonly string _qpConnectionString;
		private readonly DatabaseType _dbType;

		public StructureCacheTracker(string qpConnectionString, DatabaseType dbType)
		{
			_qpConnectionString = qpConnectionString;
			_dbType = dbType;
		}

		protected override void OnTrackChanges(List<TableModification> changes)
		{
			DbConnection connection = _dbType == DatabaseType.Postgres
				? (DbConnection)new NpgsqlConnection(_qpConnectionString)
				: new SqlConnection(_qpConnectionString); 
			using (connection)
			{
				if (connection.State != ConnectionState.Open)
					connection.Open();

				foreach (string tableNameAsTag in CacheTags.QP8.All)
				{
					string query = "select MAX(MODIFIED) from " + tableNameAsTag;
					DbCommand cmd = _dbType == DatabaseType.Postgres
						? (DbCommand)new NpgsqlCommand(query)
						: new SqlCommand(query);
					cmd.Connection = connection;
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
