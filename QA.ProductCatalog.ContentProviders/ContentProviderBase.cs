using System;
using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
	public abstract class ContentProviderBase<TModel> : IContentProvider<TModel>
		where TModel : class
	{
		protected ISettingsService SettingsService { get; }
		protected DBConnector Connector { get; set; }
		protected Customer Customer { get; }
		
		private IQpContentCacheTagNamingProvider NamingProvider;

		protected ContentProviderBase(
			ISettingsService settingsService, 
			IConnectionProvider connectionProvider,
			IQpContentCacheTagNamingProvider namingProvider,
			IUnitOfWork unitOfWork)
		{
			SettingsService = settingsService;
			Customer = connectionProvider.GetCustomer();
			NamingProvider = namingProvider;
			NamingProvider.SetUnitOfWork(unitOfWork);
		}

		protected abstract string GetSetting();
		protected abstract string GetQueryTemplate();

		protected virtual string GetQuery()
		{
			var setting = GetSetting();
			return string.IsNullOrEmpty(setting) ? null : string.Format(GetQueryTemplate(), setting);			
		}

		public virtual TModel[] GetArticles()
		{
			Connector = Customer.DbConnector;
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
							if (string.Equals(property.Name, columnName, StringComparison.InvariantCultureIgnoreCase))
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

		public virtual string[] GetTags()
		{
			int contentId = 0;
			var setting = GetSetting();
			if (!string.IsNullOrEmpty(setting) && int.TryParse(setting, out contentId))
			{
				return new []{ NamingProvider.GetByContentIds(new[] {contentId}, false)[contentId] };
			}
			return null;
		}

	}
}
