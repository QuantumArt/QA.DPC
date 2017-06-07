using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.Infrastructure
{
	public class HighloadApiUserProvider : ContentProviderBase<HighloadApiUser>
	{
		#region Constants
		private const string QueryTemplate = @"
			SELECT
				c.Name,
				c.AccessToken as Token
			FROM
				CONTENT_{0}_UNITED c
			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1";

        #endregion

        public HighloadApiUserProvider(ISettingsService settingsService, IConnectionProvider connectionProvider)
			: base(settingsService, connectionProvider)
		{
		}

		#region Overrides
		protected override string GetQuery()
		{
		    var usersContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);

            if (string.IsNullOrEmpty(usersContentId))
			{
				return null;
			}

            return string.Format(QueryTemplate, usersContentId);
		}

	    public override string[] GetTags()
	    {
	        return new[] { SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID) };
	    }
        #endregion
    }
}
