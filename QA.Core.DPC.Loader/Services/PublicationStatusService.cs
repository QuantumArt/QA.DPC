using Dapper;
using QA.Core.DPC.QP.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
        private readonly string _connectionString;

        public PublicationStatusService(IConnectionProvider connectionProvider)
        {
            _connectionString = connectionProvider.GetConnection();
        }

        public async Task<IEnumerable<ProductTimestamp>> GetProductTimestamps(int[] productIds)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var timestamps = await connection.QueryAsync<ProductTimestamp>($@"
                    SELECT
                        DpcId AS {nameof(ProductTimestamp.ProductId)},
                        IsLive AS {nameof(ProductTimestamp.IsLive)},
                        MAX(Updated) AS {nameof(ProductTimestamp.Updated)}
                    FROM dbo.Products
                    WHERE DpcId IN (@productIds)",
                    new { productIds });

                return timestamps;
            }
        }

        public async Task<IEnumerable<ProductTimestamp>> GetProductTimestamps(DateTime updatedSice)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var timestamps = await connection.QueryAsync<ProductTimestamp>($@"
                    SELECT
                        DpcId AS {nameof(ProductTimestamp.ProductId)},
                        IsLive AS {nameof(ProductTimestamp.IsLive)},
                        MAX(Updated) AS {nameof(ProductTimestamp.Updated)}
                    FROM dbo.Products
                    WHERE Updated > @updatedSice
                    GROUP BY DpcId, IsLive;",
                    new { updatedSice });

                return timestamps;
            }
        }
    }
}
