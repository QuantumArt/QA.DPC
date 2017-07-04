using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.Infrastructure
{
	public abstract class ContentProviderBase<TModel> : IContentProvider<TModel>
		where TModel : class
	{
		protected ISettingsService SettingsService { get; private set; }
		protected DBConnector Connector { get; private set; }
        protected string ConnectionString { get; private set; }

        public ContentProviderBase(ISettingsService settingsService, IConnectionProvider connectionProvider)
		{
			SettingsService = settingsService;
            ConnectionString = connectionProvider.GetConnection();
        }

		protected abstract string GetQuery();
		
		public TModel[] GetArticles()
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
					.Select(row => Converter.ToModelFromDataRow<TModel>(row))
					.ToArray();
			}
		}

	    public abstract string[] GetTags();

	}
}
