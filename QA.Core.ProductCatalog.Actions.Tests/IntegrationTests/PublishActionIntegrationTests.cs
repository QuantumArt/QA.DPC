using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore]
    [TestClass]
    public class PublishActionIntegrationTests : IntegrationTestsBase
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_Publish_Product1()
        {
            var action = ActionFactory("PublishAction");
            var context = new ActionContext { ContentItemIds = new[] { 2360 } };
            action.Process(context);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_Publish_MarketingProduct1()
        {
            var action = ActionFactory("PublishAction");
            var context = new ActionContext { ContentItemIds = new[] { 2423 } };
            action.Process(context);
        }
    }
}