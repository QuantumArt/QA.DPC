using NUnit.Framework;
using QA.Core.ProductCatalog.Actions.Exceptions;
using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestFixture]
	public class MessageResultExceptionTests : ExceptionTestsBase<MessageResultException>
	{
		private const int ProductId = 15;
		private const string Message = "Error";
		private const string ResultMessage = "Message";

		protected override MessageResultException GetException()
		{
			return new MessageResultException(ProductId, Message, MessageResult.Error(ResultMessage));
		}

		protected override void ValidateException(MessageResultException originalException, MessageResultException restoredRestored)
		{
			Assert.AreEqual(originalException.Message, restoredRestored.Message);
			Assert.AreEqual(originalException.ProductId, restoredRestored.ProductId);
			Assert.IsNotNull(restoredRestored.MessageResult);
			Assert.AreEqual(originalException.MessageResult.Text, restoredRestored.MessageResult.Text);
		}
	}
}
