using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class MarketingDeleteAction : MarketingProductAction
    {
	    public MarketingDeleteAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService) : base(getService, articleService, settingsService)
	    {
	    }

	    protected override string ActionKey => "DeleteAction";

        public override bool ExcludeArchive => false;
    }
}
