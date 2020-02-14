using NUnit.Framework;
using QA.Core.ProductCatalog.Actions.Exceptions;
//using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestFixture]
	public class ActionExceptionTests : ExceptionTestsBase<ActionException>
	{
		private const int ProductId = 15;
		private const string Message = "Error";

		protected override ActionException GetException()
		{
			var context = new ActionContext {ContentItemIds = new[] {ProductId}};
		    var innerExceptions = new[]{ new ProductException(ProductId, Message) };
			return new ActionException(Message, innerExceptions, context);
		}

		protected override void ValidateException(ActionException originalException, ActionException restoredRestored)
		{
			Assert.AreEqual(originalException.Message, restoredRestored.Message);
			Assert.IsNotNull(restoredRestored.Context);
			Assert.AreEqual(originalException.Context.ContentItemIds.Length, restoredRestored.Context.ContentItemIds.Length);
			Assert.AreEqual(originalException.Context.ContentItemIds[0], restoredRestored.Context.ContentItemIds[0]);
			Assert.IsNotNull(restoredRestored.InnerExceptions);
			Assert.AreEqual(originalException.InnerExceptions.Count, restoredRestored.InnerExceptions.Count);
			Assert.AreEqual(originalException.InnerExceptions[0].Message, restoredRestored.InnerExceptions[0].Message);
			Assert.AreEqual(((ProductException)originalException.InnerExceptions[0]).ProductId, ((ProductException)restoredRestored.InnerExceptions[0]).ProductId);
		}
	}
}
