using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Integration.DAL
{
    public class MonitoringRepository : IMonitoringRepository
    {
        public MonitoringRepository(IConnectionProvider connectionProvider)
        {
            _connectionString = connectionProvider.GetConnection();
        }

        const string SqlQuery = @" DECLARE @ids TABLE(Id int primary key) 
 DECLARE @idoc int
 EXEC sp_xml_preparedocument @idoc OUTPUT, @xmlParameter;
  
 
 INSERT INTO @ids
  SELECT * FROM OPENXML(@idoc, '/items/item')
  WITH(Id int '@id') 

SELECT p.[Id]
      ,p.[ProductType]
      ,p.[Alias]
      ,p.[Updated]
      ,p.[Hash]
      ,p.[MarketingProductId]
      ,p.[Title]
	  ,p.[UserUpdated]
	  ,p.[UserUpdatedId]
  FROM [dbo].[Products] p
  inner join @ids ids on ids.Id = p.Id";
        private readonly string _connectionString;

        public ProductInfo[] GetByIds(int[] productIDs)
        {
            Throws.IfArrayArgumentNullOrEmpty(productIDs, _ => productIDs);

            StringBuilder sb = new StringBuilder();

            sb.Append("<items>");
            foreach (var id in productIDs)
            {
                sb.AppendFormat("<item id=\"{0}\" />", id);
            }

            sb.Append("</items>");

			using (SqlConnection connection = new SqlConnection(_connectionString))
            {
	            const string sql = SqlQuery;

	            using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
					cmd.Parameters.Add((new SqlParameter("@xmlParameter", SqlDbType.NVarChar, -1) { Value = sb.ToString() }));

                    connection.Open();
                    var list = new List<ProductInfo>();
                    try
                    {
                        // производим запрос - без этого не будет работать dependency
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new ProductInfo
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Updated = Convert.ToDateTime(reader["Updated"]),
                                    Alias = Convert.ToString(reader["Alias"]),
                                    Hash = Convert.ToString(reader["Hash"]),
                                    Title = Convert.ToString(reader["Title"]),
									LastPublishedUserId = reader["UserUpdatedId"] == DBNull.Value ? null : (int?)reader["UserUpdatedId"],
									LastPublishedUserName = reader["UserUpdated"].ToString()
                                };

                                list.Add(item);
                            }
                        }

                        return list.ToArray();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }

	    public string GetProductXml(int id)
	    {
		    using (var connection = new SqlConnection(_connectionString))
		    {
			    using (SqlCommand cmd = new SqlCommand("SELECT Data FROM Products WHERE ID=@ID", connection))
			    {
				    cmd.Parameters.AddWithValue("@ID", id);

					connection.Open();

				    return (string) cmd.ExecuteScalar();
			    }
		    }
	    }

		public void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @" MERGE ProductRelevance AS TARGET
                                USING (SELECT @ProductID, @StatusID, @IsLive) AS source(ProductID, StatusID, IsLive)
                                        ON TARGET.ProductID = source.ProductID AND TARGET.IsLive = source.IsLive
                                WHEN matched THEN
                                UPDATE 
                                SET    LastUpdateTime = GETDATE(),
                                       StatusID = source.StatusID
                                WHEN NOT matched THEN
                                INSERT 
                                  (
                                    ProductID,
                                    StatusID,
									IsLive
                                  )
                                VALUES
                                  (
                                    source.ProductID,
                                    source.StatusID,
									source.IsLive
                                  );";

                var cmd = new SqlCommand(sql, connection);

                cmd.Parameters.AddWithValue("@ProductID", productId);

                cmd.Parameters.AddWithValue("@StatusID", productRelevance);

				cmd.Parameters.AddWithValue("@IsLive", isLive);

                connection.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}
