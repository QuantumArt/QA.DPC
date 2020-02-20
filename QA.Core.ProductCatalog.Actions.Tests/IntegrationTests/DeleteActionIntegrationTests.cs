using NUnit.Framework;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore("Manual")]
    [TestFixture]
    public class DeleteActionIntegrationTests : IntegrationTestsBase
    {
        [Test]
        [Category("Integration")]
        public void CastomAction_Delete_Product1()
        {
            var action = ActionFactory("DeleteAction");
            var context = new ActionContext { ContentItemIds = new[] { 13510 } };
            action.Process(context);
        }
    }
}