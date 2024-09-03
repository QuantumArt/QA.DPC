using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Persistent.Interfaces;

namespace QA.ProductCatalog.ContentProviders
{
	public class ElasticIndexProvider : ContentProviderBase<ElasticIndex>
	{
        public ElasticIndexProvider(
	        ISettingsService settingsService, 
	        IConnectionProvider connectionProvider,
	        IQpContentCacheTagNamingProvider namingProvider,
	        IUnitOfWork unitOfWork
	    )
			: base(settingsService, connectionProvider, namingProvider, unitOfWork)
        {
        }


        private string GetLanguageSetting()
        {
	        return SettingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);	        
        }
        
		#region Overrides
		
		protected override string GetSetting()
		{
			return SettingsService.GetSetting(SettingsTitles.ELASTIC_INDEXES_CONTENT_ID);	        
		}

		protected override string GetQueryTemplate()
		{
			return GetQueryTemplate(false);
		}

		public override ElasticIndex[] GetArticles()
		{
			Connector = Customer.DbConnector;
			
			var contentId = GetSetting();
			var testQuery =
				$"select count(*) From information_schema.columns where column_name = 'token' and table_name = 'content_{contentId}'";
			var testCmd = Connector.CreateDbCommand(testQuery);
			var colCount = (int)Connector.GetRealScalarData(testCmd);
			var query = GetQuery(colCount > 0);

			if (query == null)
			{
				return null;
			}

			return Connector.GetRealData(query)
				.AsEnumerable()
				.Select(ToModelFromDataRow<ElasticIndex>)
				.ToArray();
		}

		private string GetQuery(bool useToken)
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
            var languagesContentId = GetLanguageSetting();
		    var indexesContentId = GetSetting();

            if (string.IsNullOrEmpty(channelsContentId))
			{
				return null;
			}

            return string.Format(
	            GetQueryTemplate(useToken),
	            new object[] {indexesContentId, channelsContentId, languagesContentId}
	        );
		}

		private string GetQueryTemplate(bool useToken)
		{
			var languagesContentId = GetLanguageSetting();
			var useLang = !string.IsNullOrEmpty(languagesContentId);
			var language = useLang ? "COALESCE(l.Code, 'invariant')" : "'invariant'";
			var token = useToken ? "e.Token" : "''";
			var languageJoin = useLang ? "join CONTENT_{2}_UNITED l on e.Language = l.CONTENT_ITEM_ID" : "";
			var languageWhere = useLang ? " AND l.ARCHIVE = 0 AND l.VISIBLE = 1" : "";
		return @$"
	           SELECT
	                e.Name,
	                e.Address as Url,
	                e.State,
	                e.IsDefault,
	                e.DoTrace,
	                e.Date,
	                c.Url as ReindexUrl,
	                {language} as Language,
					{token} as Token
	            FROM CONTENT_{{0}}_UNITED e
	                join CONTENT_{{1}}_UNITED c ON e.ReindexChannel = c.CONTENT_ITEM_ID
	                {languageJoin}                                                    	
	            WHERE
	                c.ARCHIVE = 0 AND c.VISIBLE = 1 AND e.ARCHIVE = 0 AND e.VISIBLE = 1 {languageWhere}
                ";
		}
        #endregion
    }
}
