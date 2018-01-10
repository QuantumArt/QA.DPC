using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.Infrastructure
{
	public class ElasticIndexProvider : ContentProviderBase<ElasticIndex>
	{
		#region Constants
		private const string QueryTemplate = @"
            SELECT
	            [index].*
            FROM
                (SELECT NULL [Date]) AS temp
                CROSS APPLY
                (
                    SELECT
                        e.Name,
                        e.Address as Url,
                        e.State,
                        e.IsDefault,
                        e.DoTrace,
                        e.Date,
                        c.Url as ReindexUrl,
                        'invariant' as Language,
                    FROM
                        CONTENT_{0}_UNITED e
                        join CONTENT_{1}_UNITED c ON e.[ReindexChannel] = c.CONTENT_ITEM_ID
                    WHERE
                        c.ARCHIVE = 0 AND c.VISIBLE = 1 AND e.ARCHIVE = 0 AND e.VISIBLE = 1
                ) AS [index]";

        private const string QueryLangTemplate = @"
            SELECT
	            [index].*
            FROM
                (SELECT NULL [Date]) AS temp
                CROSS APPLY
                (
                    SELECT
                        e.Name,
                        e.Address as Url,
                        e.State,
                        e.IsDefault,
                        e.DoTrace,
                        e.Date,
                        c.Url as ReindexUrl,
                        CASE WHEN l.Code IS NULL
                            THEN 'invariant'
                            ELSE l.Code
                        END as Language
                    FROM
                        CONTENT_{0}_UNITED e
                        join CONTENT_{1}_UNITED c ON e.[ReindexChannel] = c.CONTENT_ITEM_ID
                        join CONTENT_{2}_UNITED l on e.[Language] = l.CONTENT_ITEM_ID
                    WHERE
                        c.ARCHIVE = 0 AND
                        c.VISIBLE = 1 AND
                        e.ARCHIVE = 0 AND
                        e.VISIBLE = 1 AND
                        l.ARCHIVE = 0 AND
                        l.VISIBLE = 1
                ) AS [index]";
        #endregion

        public ElasticIndexProvider(ISettingsService settingsService, IConnectionProvider connectionProvider)
			: base(settingsService, connectionProvider)
		{
		}

		#region Overrides
		protected override string GetQuery()
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
            var languagesContentId = SettingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);
		    var indexesContentId = SettingsService.GetSetting(SettingsTitles.ELASTIC_INDEXES_CONTENT_ID);

            if (string.IsNullOrEmpty(channelsContentId))
			{
				return null;
			}
			else
			{
                if (string.IsNullOrEmpty(languagesContentId))
                {
                    return string.Format(QueryTemplate, indexesContentId, channelsContentId);
                }
                else
                {
                    return string.Format(QueryLangTemplate, indexesContentId, channelsContentId, languagesContentId);
                }
			}
		}

	    public override string[] GetTags()
	    {
	        return new[] { SettingsService.GetSetting(SettingsTitles.ELASTIC_INDEXES_CONTENT_ID) };
	    }
        #endregion
    }
}
