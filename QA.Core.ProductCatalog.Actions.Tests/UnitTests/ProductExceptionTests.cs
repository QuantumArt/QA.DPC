using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Exceptions;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestClass]
	public class ProductExceptionTests : ExceptionTestsBase<ProductException>
	{
		private const int ProductId = 15;
		private const string Message = "Error";

		protected override ProductException GetException()
		{
			return new ProductException(ProductId, Message);
		}

		protected override void ValidateException(ProductException originalException, ProductException restoredRestored)
		{
			Assert.AreEqual(originalException.Message, restoredRestored.Message);
			Assert.AreEqual(originalException.ProductId, restoredRestored.ProductId);
		}
	}
}
