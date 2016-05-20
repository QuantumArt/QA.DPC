using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class MarketingDeleteAction : MarketingProductAction
    {
	    public MarketingDeleteAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService) : base(getService, articleService, settingsService)
	    {
	    }

	    protected override string ActionKey { get { return "DeleteAction"; } }
    }
}
