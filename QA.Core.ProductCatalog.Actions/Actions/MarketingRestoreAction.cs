using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class MarketingRestoreAction : MarketingProductActionBase
    {
        public MarketingRestoreAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService) : base(getService, articleService, settingsService)
        {
        }

        protected override string ActionKey => "RestoreAction";

        public override bool ExcludeArchive => false;
    }
}
