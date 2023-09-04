using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;

namespace QA.ProductCatalog.ContentProviders
{
	public class HighloadApiUserProvider : ContentProviderBase<HighloadApiUser>
	{
		public HighloadApiUserProvider(
			ISettingsService settingsService,
			IConnectionProvider connectionProvider,
			IQpContentCacheTagNamingProvider namingProvider
		)
			: base(settingsService, connectionProvider, namingProvider)
		{
		}

		#region Overrides

		protected override string GetSetting()
		{
			return SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);
		}

		public override string[] GetTags()
		{
			return new[] {GetSetting()};
		}

		protected override string GetQueryTemplate()
		{
			return @"
				SELECT
					c.Name,
					c.AccessToken as Token
				FROM
					CONTENT_{0}_UNITED c
				WHERE
					c.ARCHIVE = 0 AND c.VISIBLE = 1
			";
		}
		
		#endregion
	
    }
}
