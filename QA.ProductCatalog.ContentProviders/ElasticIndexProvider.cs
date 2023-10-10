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

        
        protected string GetLanguageSetting()
        {
	        return SettingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);	        
        }
        
		#region Overrides
		
		protected override string GetSetting()
		{
			return SettingsService.GetSetting(SettingsTitles.ELASTIC_INDEXES_CONTENT_ID);	        
		}
		
		protected override string GetQuery()
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
            var languagesContentId = GetLanguageSetting();
		    var indexesContentId = GetSetting();

            if (string.IsNullOrEmpty(channelsContentId))
			{
				return null;
			}

            return string.Format(
	            GetQueryTemplate(),
	            new object[] {indexesContentId, channelsContentId, languagesContentId}
	        );
		}

		protected override string GetQueryTemplate()
		{
			var languagesContentId = GetLanguageSetting();
			var useLang = !string.IsNullOrEmpty(languagesContentId);
			var language = useLang ? "COALESCE(l.Code, 'invariant')" : "'invariant'";
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
	                {language} as Language
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
