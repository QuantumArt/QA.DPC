using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QA.Core;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.Front;
using QA.Core.DPC.Front.DAL;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;
using ProductInfo = QA.ProductCatalog.Infrastructure.ProductInfo;

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

        private DpcModelDataContext GetNpgSqlDpcModelDataContext(string connectionString)
        {
	        var builder = new DbContextOptionsBuilder<NpgSqlDpcModelDataContext>();
	        builder.UseNpgsql(connectionString);
	        return new NpgSqlDpcModelDataContext(builder.Options);
        }

        private DpcModelDataContext GetSqlServerDpcModelDataContext(string connectionString)
        {
	        var builder = new DbContextOptionsBuilder<SqlServerDpcModelDataContext>();
	        builder.UseSqlServer(connectionString);
	        return new SqlServerDpcModelDataContext(builder.Options);
        }
        
        public ProductInfo[] GetByIds(int[] productIDs)
        {
            Throws.IfArrayArgumentNullOrEmpty(productIDs, _ => productIDs);

            using(DpcModelDataContext context = _customer.DatabaseType == DatabaseType.Postgres
	            ? GetNpgSqlDpcModelDataContext(_customer.ConnectionString)
	            : GetSqlServerDpcModelDataContext(_customer.ConnectionString))
            {
	            var products = context.GetProducts(new ProductLocator
	            {
		            Language = _language,
		            IsLive = _state,
		            Version = 1
	            }).Where(p =>
					p.Format == (_isJson ? "json" : "xml")
					&& string.IsNullOrEmpty(p.Slug)
					&& productIDs.Contains(p.Id)
	            );
	            //INNER JOIN (SELECT Id FROM {idsExpression}) AS ids ON ids.Id = p.DpcId 
                var list = new List<ProductInfo>();
                // производим запрос - без этого не будет работать dependency
                foreach (var product in products)
                {
                    var item = new ProductInfo
                    {
                        Id = product.Id,
                        Updated = product.Updated,
                        Alias = product.Alias,
                        Hash = product.Hash,
                        Title = product.Title,
						LastPublishedUserId = product.UserUpdatedId,
						LastPublishedUserName = product.UserUpdated
                    };

                    list.Add(item);
                }

                return list.ToArray();
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
