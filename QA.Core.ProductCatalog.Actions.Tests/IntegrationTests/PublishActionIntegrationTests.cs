using NUnit.Framework;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore("Manual")]
    [TestFixture]
    public class PublishActionIntegrationTests : IntegrationTestsBase
    {
        [Test]
        [Category("Integration")]
        public void CastomAction_Publish_Product1()
        {
            var action = ActionFactory("PublishAction");
            var context = new ActionContext { ContentItemIds = new[] { 2360 } };
            action.Process(context);
        }

        [Test]
        [Category("Integration")]
        public void CastomAction_Publish_MarketingProduct1()
        {
            var action = ActionFactory("PublishAction");
            var context = new ActionContext { ContentItemIds = new[] { 2423 } };
            action.Process(context);
        }
    }
}