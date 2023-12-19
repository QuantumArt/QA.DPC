using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Persistent.Interfaces;
using Quantumart.QPublishing.Database;
using DatabaseType = QP.ConfigurationService.Models.DatabaseType;

namespace QA.ProductCatalog.ContentProviders
{
	public class HighloadApiLimitProvider : ContentProviderBase<HighloadApiLimit>
	{
		private DatabaseType _dbType;

        public HighloadApiLimitProvider(
	        ISettingsService settingsService, 
	        IConnectionProvider connectionProvider,
	        IQpContentCacheTagNamingProvider namingProvider,
	        IUnitOfWork unitOfWork
	    ): base(settingsService, connectionProvider, namingProvider, unitOfWork)
        {
	        _dbType = connectionProvider.GetCustomer().DatabaseType;
        }

        protected override string GetSetting()
        {
	        return SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_LIMITS_CONTENT_ID);	        
        }

		#region Overrides
		protected override string GetQuery()
		{
		    var limitsContentId = GetSetting();
		    var methodsContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_METHODS_CONTENT_ID);
		    var usersContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_USERS_CONTENT_ID);


            if (string.IsNullOrEmpty(limitsContentId) || string.IsNullOrEmpty(methodsContentId) || string.IsNullOrEmpty(usersContentId))
			{
				return null;
			}

            var user = SqlQuerySyntaxHelper.EscapeEntityName(_dbType, "User");

            return string.Format(GetQueryTemplate(), limitsContentId, methodsContentId, usersContentId, user);
		}

	    protected override string GetQueryTemplate()
	    {
		    return @"
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
					c.ARCHIVE = 0 AND c.VISIBLE = 1 AND m.ARCHIVE = 0 AND m.VISIBLE = 1 AND u.ARCHIVE = 0 AND u.VISIBLE = 1
			";
	    }

	    #endregion
	}
}
