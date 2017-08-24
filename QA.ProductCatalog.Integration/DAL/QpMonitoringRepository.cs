using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using QA.Core;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.DAL;

namespace QA.ProductCatalog.Integration.DAL
{
    public class QpMonitoringRepository : IMonitoringRepository
    {
        public QpMonitoringRepository(IConnectionProvider connectionProvider, IArticleFormatter formatter, bool state)
            : this(connectionProvider, formatter, state, null)
        {
        }

        public QpMonitoringRepository(IConnectionProvider connectionProvider, IArticleFormatter formatter, bool state, string language)
        {
            _connectionString = connectionProvider.GetConnection();
            _state = state;
            _language = String.IsNullOrEmpty(language) ? "invariant" : language;
            _isJson = formatter is JsonProductFormatter;
        }


        private readonly string _connectionString;

        private readonly bool _state;

        private readonly string _language;

        private readonly bool _isJson;

        private string GetSqlQuery()
        {
            return @" 
            SELECT DpcId as Id, ProductType, Alias, Updated, Hash, MarketingProductId, Title, UserUpdated, UserUpdatedId 
            FROM [dbo].[Products] p INNER JOIN @ids ids ON ids.Id = p.DpcId 
            WHERE p.IsLive = @isLive AND p.Language = @language 
            AND p.Format = @format AND p.Version = 1 and p.Slug is null";
        }

        public ProductInfo[] GetByIds(int[] productIDs)
        {
            Throws.IfArrayArgumentNullOrEmpty(productIDs, _ => productIDs);

			using (SqlConnection connection = new SqlConnection(_connectionString))
            {
	            using (SqlCommand cmd = new SqlCommand(GetSqlQuery(), connection))
	            {
	                var p = new SqlParameter("@ids", SqlDbType.Structured)
	                {
	                    TypeName = "Ids",
	                    Value = Common.IdsToDataTable(productIDs)
	                };

                    cmd.Parameters.Add(p);
                    cmd.Parameters.AddWithValue("@isLive", _state ? 1 : 0);
	                cmd.Parameters.AddWithValue("@language", _language);
	                cmd.Parameters.AddWithValue("@format", _isJson ? "json" : "xml");

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
			    using (SqlCommand cmd = new SqlCommand(
                    "SELECT Data FROM Products p WHERE p.DpcId = @id AND p.IsLive = @isLive AND p.Language = @language " +
			        "AND p.Format = @format AND p.Version = 1 and p.Slug is null", connection))
			    {
				    cmd.Parameters.AddWithValue("@id", id);
			        cmd.Parameters.AddWithValue("@isLive", _state ? 1 : 0);
			        cmd.Parameters.AddWithValue("@language", _language);
			        cmd.Parameters.AddWithValue("@format", _isJson ? "json" : "xml");

                    connection.Open();

				    return (string) cmd.ExecuteScalar();
			    }
		    }
	    }

		public void InsertOrUpdateProductRelevanceStatus(int productId, ProductRelevance productRelevance, bool isLive)
		{
		    throw new NotImplementedException();
		}
    }
}
