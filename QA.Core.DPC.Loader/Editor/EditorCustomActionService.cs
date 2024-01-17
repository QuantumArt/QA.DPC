using Dapper;
using QA.Core.DPC.QP.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Npgsql;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

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
        private readonly Customer _customer;

        public EditorCustomActionService(IConnectionProvider connectionProvider)
        {
            _customer = connectionProvider.GetCustomer();
        }

        public async Task<CustomActionInfo> GetCustomActionByAlias(string alias)
        {
            if (String.IsNullOrWhiteSpace(alias))
            {
                throw new ArgumentException(nameof(alias));
            }

            DbConnection connection = _customer.DatabaseType == DatabaseType.Postgres
                ? new NpgsqlConnection(_customer.ConnectionString)
                : new SqlConnection(_customer.ConnectionString); 
            
            using (connection)
            {
                await connection.OpenAsync();

                var actionInfo = await connection.QuerySingleAsync<CustomActionInfo>($@"
                    SELECT {SqlQuerySyntaxHelper.Top(_customer.DatabaseType, "1")}
                        ba.CODE AS {nameof(CustomActionInfo.ActionCode)},
	                    et.CODE AS {nameof(CustomActionInfo.EntityTypeCode)}
                    FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}CUSTOM_ACTION AS ca
                    INNER JOIN {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}BACKEND_ACTION AS ba ON ca.ACTION_ID = ba.ID
                    INNER JOIN {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}ENTITY_TYPE AS et ON ba.ENTITY_TYPE_ID = et.ID
                    WHERE ca.ALIAS = @{nameof(alias)} {SqlQuerySyntaxHelper.Limit(_customer.DatabaseType, "1")}",
                    new { alias });
                
                return actionInfo;
            }
        }
    }
}
