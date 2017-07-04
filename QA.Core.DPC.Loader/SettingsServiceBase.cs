using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using QA.Core.DPC.Loader.Resources;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using QA.Core.Web;

namespace QA.Core.DPC.Loader
{
	public abstract class SettingsServiceBase : ISettingsService
	{
		private static readonly RequestLocal<Dictionary<string, string>> Actions =
			new RequestLocal<Dictionary<string, string>>(() => new Dictionary<string, string>());

		protected readonly string _connectionString;

		protected SettingsServiceBase(IConnectionProvider connectionProvider)
		{
			_connectionString = connectionProvider.GetConnection();
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

			Dictionary<string, string> d = Actions.Value ?? new Dictionary<string, string>();

			string result;

			if (d.TryGetValue(name, out result))
			{
				d[name] = result;
			}
			else
			{
				result = GetActionCodeInternal(name);
				d[name] = result;
			}

			Actions.Value = d;

			return result;
		}

		private string GetActionCodeInternal(string name)
		{
			string sql = @"select  top 1 [CODE], [NAME],[ID] from backend_action where name like @name";
			using (var cs = new QPConnectionScope(_connectionString))
			{
				SqlConnection con = cs.DbConnection;

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