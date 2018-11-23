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

        public async Task<CustomActionInfo> GetCustomActionByName(string actionName)
        {
            if (String.IsNullOrWhiteSpace(actionName))
            {
                throw new ArgumentException(nameof(actionName));
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var actionInfo = await connection.QuerySingleAsync<CustomActionInfo>($@"
                    SELECT TOP (1)
                        a.CODE AS {nameof(CustomActionInfo.ActionCode)},
	                    t.CODE AS {nameof(CustomActionInfo.EntityTypeCode)}
                    FROM dbo.BACKEND_ACTION AS a
                    INNER JOIN dbo.ENTITY_TYPE AS t ON a.ENTITY_TYPE_ID = t.ID
                    WHERE a.NAME = @{nameof(actionName)}",
                    new { actionName });

                return actionInfo;
            }
        }
    }
}
