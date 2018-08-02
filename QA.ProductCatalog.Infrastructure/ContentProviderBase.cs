using System;
using System.Data;
using System.Linq;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.Core.Cache;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Infrastructure
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
			Connector = QPConnectionScope.Current == null ? new DBConnector(ConnectionString) : new DBConnector(QPConnectionScope.Current.DbConnection);
			var query = GetQuery();

			if (query == null)
			{
				return null;
			}
			else
			{
				return Connector.GetRealData(query)
					.AsEnumerable()
					.Select(Converter.ToModelFromDataRow<TModel>)
					.ToArray();
			}
		}

	    public abstract string[] GetTags();

	}
}
