﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.QP.Cache
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
			using (var con = new SqlConnection(_qpConnectionString))
			{
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