using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class MarketingArchiveAction : MarketingProductAction
    {
        public MarketingArchiveAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService) : base(getService, articleService, settingsService)
        {
        }

        protected override string ActionKey { get { return "ArchiveAction"; } }
    }
}
