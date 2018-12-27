using System;
using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Services;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
	public abstract class ContentProviderBase<TModel> : IContentProvider<TModel>
		where TModel : class
	{
		protected ISettingsService SettingsService { get; }
		private DBConnector Connector { get; set; }
		private string ConnectionString { get; }

		protected ContentProviderBase(ISettingsService settingsService, IConnectionProvider connectionProvider)
		{
			SettingsService = settingsService;
            ConnectionString = connectionProvider.GetConnection();
		}

		protected abstract string GetQuery();

		public virtual TModel[] GetArticles()
		{
			Connector = new DBConnector(ConnectionString);
			var query = GetQuery();

			if (query == null)
			{
				return null;
			}
			else
			{
				return Connector.GetRealData(query)
					.AsEnumerable()
					.Select(ToModelFromDataRow<TModel>)
					.ToArray();
			}
		}
		
		public static T ToModelFromDataRow<T>(DataRow row)
			where T : class
		{
			try
			{
				var model = Activator.CreateInstance<T>();

				foreach (var property in typeof(T).GetProperties())
				{
					foreach (DataColumn key in row.Table.Columns)
					{
						var columnName = key.ColumnName;
						if (!string.IsNullOrEmpty(row[columnName].ToString()))
						{
							if (property.Name == columnName)
							{
								var t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
								var safeValue = row[columnName] == null ? null : Convert.ChangeType(row[columnName], t);
								property.SetValue(model, safeValue, null);
							}
						}
					}
				}

				return model;
			}
			catch (MissingMethodException)
			{
				return null;
			}
		}

	    public abstract string[] GetTags();

	}
}
