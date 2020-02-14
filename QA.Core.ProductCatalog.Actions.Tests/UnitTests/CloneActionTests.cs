using NUnit.Framework;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestFixture]
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
