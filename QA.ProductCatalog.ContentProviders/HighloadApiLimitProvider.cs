using QA.Core.DPC.QP.Services;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
	public class HighloadApiLimitProvider : ContentProviderBase<HighloadApiLimit>
	{
		private DatabaseType _dbType;
		#region Constants
		private const string QueryTemplate = @"
			SELECT
				u.Name as {3},
                m.Title as Method,
                c.Seconds,
                c.Limit
			FROM
				CONTENT_{0}_UNITED c
				join CONTENT_{1}_UNITED m ON c.ApiMethod = m.CONTENT_ITEM_ID
				join CONTENT_{2}_UNITED u ON c.{3} = u.CONTENT_ITEM_ID

			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1 AND m.ARCHIVE = 0 AND m.VISIBLE = 1 AND u.ARCHIVE = 0 AND u.VISIBLE = 1";

        #endregion

        public HighloadApiLimitProvider(ISettingsService settingsService, IConnectionProvider connectionProvider)
			: base(settingsService, connectionProvider)
        {
	        _dbType = connectionProvider.GetCustomer().DatabaseType;
        }

		#region Overrides
		protected override string GetQuery()
		{
		    var limitsContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_LIMITS_CONTENT_ID);
		    var methodsContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_METHODS_CONTENT_ID);
		    var usersContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);


            if (string.IsNullOrEmpty(limitsContentId) || string.IsNullOrEmpty(methodsContentId) || string.IsNullOrEmpty(usersContentId))
			{
				return null;
			}

            var user = SqlQuerySyntaxHelper.EscapeEntityName(_dbType, "User");

            return string.Format(QueryTemplate, limitsContentId, methodsContentId, usersContentId, user);
		}

	    public override string[] GetTags()
	    {
	        return new []{ SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_LIMITS_CONTENT_ID) };
	    }

	    #endregion
	}
}
