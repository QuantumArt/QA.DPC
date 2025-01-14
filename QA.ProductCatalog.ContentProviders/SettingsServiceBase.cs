using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
    public abstract class SettingsServiceBase : ISettingsService
    {
        protected readonly Customer _customer;

        protected ICacheProvider CacheProvider { get; }

        protected SettingsServiceBase(
            IConnectionProvider connectionProvider,
            ICacheProvider cacheProvider)
        {
            _customer = connectionProvider.GetCustomer();
            CacheProvider = cacheProvider;
        }

        public string GetSetting(SettingsTitles title)
        {
            return GetSetting(title.ToString());
        }

        public abstract string GetSetting(string title);

        public string GetActionCode(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string key = $"Actions_[{name}]";
            string result = CacheProvider.Get<string>(key);
            if (result == null)
            {
                result = GetActionCodeInternal(name);
                CacheProvider.Set(key, result, TimeSpan.FromMinutes(5));
            }
            return result;
        }

        private string GetActionCodeInternal(string name)
        {
            string sql = $@"select {SqlQuerySyntaxHelper.Top(_customer.DatabaseType, "1")} CODE, NAME, ID 
				FROM backend_action 
				WHERE name like @name {SqlQuerySyntaxHelper.Limit(_customer.DatabaseType, "1")}";
            if (_customer.DatabaseType == DatabaseType.SqlServer)
            {
                SqlConnection connection = new SqlConnection(_customer.ConnectionString);
                using (connection)
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    var codes = new List<string>();
                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        var idParameter = new SqlParameter();
                        idParameter.ParameterName = "@name"; // Defining Name
                        idParameter.SqlDbType = SqlDbType.NVarChar; // Defining DataType
                        idParameter.Direction = ParameterDirection.Input; // Setting the direction 
                        idParameter.Value = name;

                        cmd.Parameters.Add(idParameter);

                        using (SqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                codes.Add(rd.GetString(0));
                            }

                            rd.Close();
                        }
                    }

                    return codes.FirstOrDefault();
                }
            }
            else
            {
                NpgsqlConnection connection = new NpgsqlConnection(_customer.ConnectionString);
                using (connection)
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();
                    var codes = new List<string>();
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        var idParameter = new NpgsqlParameter();
                        idParameter.ParameterName = "@name"; // Defining Name
                        idParameter.NpgsqlDbType = NpgsqlDbType.Text; // Defining DataType
                        idParameter.Direction = ParameterDirection.Input; // Setting the direction 
                        idParameter.Value = name;

                        cmd.Parameters.Add(idParameter);

                        using (NpgsqlDataReader rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                codes.Add(rd.GetString(0));
                            }

                            rd.Close();
                        }
                    }

                    return codes.FirstOrDefault();
                }
            }
        }
    }
}