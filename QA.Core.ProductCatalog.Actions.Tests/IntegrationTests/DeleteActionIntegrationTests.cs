using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore]
    [TestClass]
    public class DeleteActionIntegrationTests : IntegrationTestsBase
    {
        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_Delete_Product1()
        {
            var action = ActionFactory("DeleteAction");
            var context = new ActionContext { ContentItemIds = new[] { 13510 } };
            action.Process(context);
        }
    }
}