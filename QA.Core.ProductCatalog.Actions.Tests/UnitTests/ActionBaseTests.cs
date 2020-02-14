using NUnit.Framework;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using System;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestFixture]
	public class ActionBaseTests
	{
		#region Constants
		private const int ContentItemId = 100;
		#endregion

		#region Private properties
		private IArticleService ArticleService { get; set; }
		private IFieldService FieldService { get; set; }
		private ProductServiceFake ProductService { get; set; }
		private LoggerFake Logger { get; set; }
		Func<ITransaction> TransactionFactory { get; set; }
		private ActionBaseFake Action { get; set; }
		private ActionContext Context { get; set; }
		#endregion

		#region Initialization
		[SetUp]
		public void Initialize()
		{
			ArticleService = new ArticleServiceFake();
			FieldService = new FieldServiceFake();
            ProductService = new ProductServiceFake();
			Logger = new LoggerFake();
			TransactionFactory = () => new TransactionFake();
			Action = new ActionBaseFake(ArticleService, FieldService, ProductService, TransactionFactory);
			Context = new ActionContext {ContentItemIds = new[] {ContentItemId}};
		}
		#endregion

		#region Test methods
		[Test]
		public void Constructor_ArticleServiceIsNull_ThrowException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var action = new ActionBaseFake(null, FieldService, ProductService, TransactionFactory);
			});
		}

		[Test]
		public void Constructor_ProductServiceIsNull_ThrowException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var action = new ActionBaseFake(ArticleService, FieldService, null, TransactionFactory);
			});
		}

		[Test]
		public void Constructor_TransactionFactoryIsNull_ThrowException()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var action = new ActionBaseFake(ArticleService, FieldService, ProductService, null);
			});
		}

		[Test]
		public void Process_ContentItemId_PassToProcessProduct()
		{
			Action.Process(Context);
			Assert.AreEqual(Context.ContentItemIds[0], Action.LastProductId);
		}

		[Test]
		public void Process_CatchException_ThrowActionException()
		{
			Action.ExceptionToThrow = new Exception();

			try
			{
				Action.Process(Context);
			}
			catch (ActionException ex)
			{
				Assert.AreEqual(Action.ExceptionToThrow, ex.InnerExceptions[0].InnerException);
				Assert.AreEqual(Context, ex.Context);
			}
		}
		#endregion
	}
}