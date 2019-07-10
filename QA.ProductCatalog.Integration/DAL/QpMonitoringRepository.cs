using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Npgsql;
using QA.Core;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

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
	        _customer = connectionProvider.GetCustomer();
            _state = state;
            _language = String.IsNullOrEmpty(language) ? "invariant" : language;
            _isJson = formatter is JsonProductFormatter;
        }


        private readonly Customer _customer;

        private readonly bool _state;

        private readonly string _language;

        private readonly bool _isJson;

        private string GetSqlQuery(string idsExpression)
        {
            return $@"SELECT DpcId as Id, ProductType, Alias, Updated, Hash, MarketingProductId, Title, UserUpdated, UserUpdatedId 
            FROM {SqlQuerySyntaxHelper.DbSchemaName(_customer.DatabaseType)}.Products p 
			INNER JOIN (SELECT Id FROM {idsExpression}) AS ids ON ids.Id = p.DpcId 
            WHERE p.IsLive = @isLive AND p.Language = @language 
            AND p.Format = @format AND p.Version = 1 and p.Slug is null";
        }

        public ProductInfo[] GetByIds(int[] productIDs)
        {
            Throws.IfArrayArgumentNullOrEmpty(productIDs, _ => productIDs);

            DbConnection connection = _customer.DatabaseType == DatabaseType.SqlServer
	            ? (DbConnection)new SqlConnection(_customer.ConnectionString)
	            : new NpgsqlConnection(_customer.ConnectionString);
            
			using (connection)
			{
				var idList = SqlQuerySyntaxHelper.IdList(_customer.DatabaseType, "@ids", "ids");
				var query = GetSqlQuery(idList);
				DbCommand cmd = _customer.DatabaseType == DatabaseType.SqlServer
					? (DbCommand) new SqlCommand(query)
					: new NpgsqlCommand(query);
	            cmd.Connection = connection;
	            using (cmd)
	            {
		            cmd.Parameters.AddWithValue("@ids", productIDs);
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
		    DbConnection connection = _customer.DatabaseType == DatabaseType.SqlServer
			    ? (DbConnection)new SqlConnection(_customer.ConnectionString)
			    : new NpgsqlConnection(_customer.ConnectionString);
		    using (connection)
		    {
			    var query = @"SELECT Data FROM Products p 
					WHERE p.DpcId = @id 
						AND p.IsLive = @isLive AND p.Language = @language 
				    	AND p.Format = @format AND p.Version = 1 and p.Slug is null";
			    DbCommand cmd = _customer.DatabaseType == DatabaseType.SqlServer
				    ? (DbCommand) new SqlCommand(query)
				    : new NpgsqlCommand(query);
			    cmd.Connection = connection;
			    using (cmd)
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
