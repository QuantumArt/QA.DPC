using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class MarketingRestoreAction : MarketingProductAction
    {
        public MarketingRestoreAction(Func<string, IAction> getService, IArticleService articleService, ISettingsService settingsService) : base(getService, articleService, settingsService)
        {
        }

        protected override string ActionKey { get { return "RestoreAction"; } }
    }
}
