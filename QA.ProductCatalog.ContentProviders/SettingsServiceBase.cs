using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using QA.Core;
using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.ContentProviders
{
    public abstract class SettingsServiceBase : ISettingsService
	{

		protected readonly string _connectionString;
		protected readonly ICacheProvider _provider;

		protected SettingsServiceBase(IConnectionProvider connectionProvider, ICacheProvider provider)
		{
			_connectionString = connectionProvider.GetConnection();
			_provider = provider;
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
			string result = (string)_provider.Get(key);
			if (result == null)
			{
				result = GetActionCodeInternal(name);
				_provider.Set(key, result, TimeSpan.FromMinutes(5));
			}
			return result;
		}

		private string GetActionCodeInternal(string name)
		{
			string sql = @"select  top 1 [CODE], [NAME],[ID] from backend_action where name like @name";
			using (var con = new SqlConnection(_connectionString))
			{
				if (con.State != ConnectionState.Open)
					con.Open();
				var codes = new List<string>();
				using (var cmd = new SqlCommand(
					sql, con))
				{
					var idParametr = new SqlParameter();
					idParametr.ParameterName = "@name"; // Defining Name
					idParametr.SqlDbType = SqlDbType.NVarChar; // Defining DataType
					idParametr.Direction = ParameterDirection.Input; // Setting the direction 
					idParametr.Value = name;

					cmd.Parameters.Add(idParametr);

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
	}
}