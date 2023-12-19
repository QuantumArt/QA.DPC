using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using QA.DotNetCore.Engine.Persistent.Interfaces;

namespace QA.ProductCatalog.ContentProviders
{
    public class HighloadApiMethodProvider : ContentProviderBase<HighloadApiMethod>
    {
	    public HighloadApiMethodProvider(
		    ISettingsService settingsService, 
		    IConnectionProvider connectionProvider, 
		    IQpContentCacheTagNamingProvider namingProvider,
		    IUnitOfWork unitOfWork)
		    : base(settingsService, connectionProvider, namingProvider, unitOfWork)
	    {
	    }

	    #region Overrides

	    protected override string GetSetting()
	    {
		    return SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_METHODS_CONTENT_ID);
	    }

	    protected override string GetQueryTemplate()
	    {
		    return @"
				SELECT
	                c.Title,
	                c.System,
	                c.Json
				FROM
					CONTENT_{0}_UNITED c

				WHERE
					c.ARCHIVE = 0 AND c.VISIBLE = 1 ";
	    }
	    
	    #endregion
	    
    }
}