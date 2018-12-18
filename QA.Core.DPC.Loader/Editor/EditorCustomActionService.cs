using Dapper;
using QA.Core.DPC.QP.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace QA.Core.DPC.Loader.Editor
{
    #region Models

    public class CustomActionInfo
    {
        public string ActionCode { get; set; }
        public string EntityTypeCode { get; set; }
    }

    #endregion

    public class EditorCustomActionService
    {
        private readonly string _connectionString;

        public EditorCustomActionService(IConnectionProvider connectionProvider)
        {
            _connectionString = connectionProvider.GetConnection();
        }

        public async Task<CustomActionInfo> GetCustomActionByAlias(string alias)
        {
            if (String.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException(nameof(alias));
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var actionInfo = await connection.QuerySingleAsync<CustomActionInfo>($@"
                    SELECT TOP (1)
                        ba.CODE AS {nameof(CustomActionInfo.ActionCode)},
	                    et.CODE AS {nameof(CustomActionInfo.EntityTypeCode)}
                    FROM dbo.CUSTOM_ACTION AS ca
                    INNER JOIN dbo.BACKEND_ACTION AS ba ON ca.ACTION_ID = ba.ID
                    INNER JOIN dbo.ENTITY_TYPE AS et ON ba.ENTITY_TYPE_ID = et.ID
                    WHERE ca.ALIAS = @{nameof(alias)}",
                    new { alias });
                
                return actionInfo;
            }
        }
    }
}
