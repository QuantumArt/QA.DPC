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

namespace QA.Core.DPC.Loader.Services
{
    #region Models

    public class ProductTimestamp
    {
        public int ProductId { get; set; }
        public bool IsLive { get; set; }
        public DateTime Updated { get; set; }
    }

    #endregion

    public class PublicationStatusService
    {
        private readonly Customer _customer;

        public PublicationStatusService(IConnectionProvider connectionProvider)
        {
            _customer = connectionProvider.GetCustomer();
        }

        public async Task<DateTime?> GetMaxPublicationTime()
        {
            DbConnection connection = _customer.DatabaseType == DatabaseType.Postgres
                ? (DbConnection)new NpgsqlConnection(_customer.ConnectionString)
                : new SqlConnection(_customer.ConnectionString); 
            using (connection)
            {
                await connection.OpenAsync();

                DateTime? timestamp = await connection.QuerySingleAsync<DateTime?>($@"
                    SELECT {SqlQuerySyntaxHelper.Top(_customer.DatabaseType, "1")}
                        Updated
                    FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}Products
                    ORDER BY Updated DESC {SqlQuerySyntaxHelper.Limit(_customer.DatabaseType, "1")}");

                return timestamp;
            }
        }

        public async Task<IEnumerable<ProductTimestamp>> GetProductTimestamps(int[] productIds)
        {
            DbConnection connection = _customer.DatabaseType == DatabaseType.Postgres
                ? (DbConnection)new NpgsqlConnection(_customer.ConnectionString)
                : new SqlConnection(_customer.ConnectionString); 
            using (connection)
            {
                await connection.OpenAsync();

                var timestamps = await connection.QueryAsync<ProductTimestamp>($@"
                    SELECT
                        DpcId AS {nameof(ProductTimestamp.ProductId)},
                        IsLive AS {nameof(ProductTimestamp.IsLive)},
                        Updated AS {nameof(ProductTimestamp.Updated)}
                    FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}Products
                    WHERE DpcId IN @{nameof(productIds)}",
                    new { productIds });

                return timestamps;
            }
        }

        public async Task<IEnumerable<ProductTimestamp>> GetProductTimestamps(DateTime updatedSince)
        {
            DbConnection connection = _customer.DatabaseType == DatabaseType.Postgres
                ? (DbConnection)new NpgsqlConnection(_customer.ConnectionString)
                : new SqlConnection(_customer.ConnectionString); 
            using (connection)
            {
                await connection.OpenAsync();

                var timestamps = await connection.QueryAsync<ProductTimestamp>($@"
                    SELECT
                        DpcId AS {nameof(ProductTimestamp.ProductId)},
                        IsLive AS {nameof(ProductTimestamp.IsLive)},
                        Updated AS {nameof(ProductTimestamp.Updated)}
                    FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}Products
                    WHERE Updated > @{nameof(updatedSince)}",
                    new { updatedSince });

                return timestamps;
            }
        }
    }
}
