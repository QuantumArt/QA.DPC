using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore]
    [TestClass]
    public class CloneActionIntegrationTests : IntegrationTestsBase
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_Clone_Product1()
        {
            var action = ActionFactory("CloneAction");
            var context = new ActionContext { ContentItemIds = new[] { 2360 } };
            action.Process(context);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_Clone_MarketingProduct1()
        {
            var action = ActionFactory("CloneAction");
            var context = new ActionContext { ContentItemIds = new[] { 2423 } };
            action.Process(context);
        }
    }
}