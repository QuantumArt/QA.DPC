using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestClass]
	public class CloneActionTests : ActionTestsBase
	{
		#region Overrides
		protected override void InitializeAction()
		{
			Action = new CloneAction(ArticleService, FieldService, ProductService, CreateTransaction, null);
		}
		#endregion
	}
}
