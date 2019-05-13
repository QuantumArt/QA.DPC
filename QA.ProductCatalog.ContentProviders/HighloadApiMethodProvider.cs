using QA.Core.DPC.QP.Services;

namespace QA.ProductCatalog.ContentProviders
{
    public class HighloadApiMethodProvider : ContentProviderBase<HighloadApiMethod>
    {
        
        #region Constants
        private const string QueryTemplate = @"
			SELECT
                c.Title,
                c.System,
                c.Json
			FROM
				CONTENT_{0}_UNITED c

			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1 ";

        #endregion
	    
	    public HighloadApiMethodProvider(ISettingsService settingsService, IConnectionProvider connectionProvider)
		    : base(settingsService, connectionProvider)
	    {
	
	    }
	    
	    #region Overrides
	    protected override string GetQuery()
	    {
		    var methodsContentId = SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_METHODS_CONTENT_ID);


		    if (string.IsNullOrEmpty(methodsContentId))
		    {
			    return null;
		    }

		    return string.Format(QueryTemplate, methodsContentId);
	    }

	    public override string[] GetTags()
	    {
		    return new []{ SettingsService.GetSetting(SettingsTitles.HIGHLOAD_API_METHODS_CONTENT_ID) };
	    }

	    #endregion
        
    }
	


}