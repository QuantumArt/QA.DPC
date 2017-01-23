using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using System;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestClass]
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
		[TestInitialize]
		public void Initialize()
		{
			ArticleService = new ArticleServiceFake();
			FieldService = new FieldServiceFake();
            ProductService = new ProductServiceFake();
			Logger = new LoggerFake();
			TransactionFactory = () => new TransactionFake();
			Action = new ActionBaseFake(ArticleService, FieldService, ProductService, Logger, TransactionFactory);
			Context = new ActionContext {ContentItemIds = new[] {ContentItemId}};
		}
		#endregion

		#region Test methods
		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_ArticleServiceIsNull_ThrowException()
		{
			var action = new ActionBaseFake(null, FieldService, ProductService, Logger, TransactionFactory);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_ProductServiceIsNull_ThrowException()
		{
			var action = new ActionBaseFake(ArticleService, FieldService, null, Logger, TransactionFactory);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_LoggerIsNull_ThrowException()
		{
			var action = new ActionBaseFake(ArticleService, FieldService, ProductService, null, TransactionFactory);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_TransactionFactoryIsNull_ThrowException()
		{
			var action = new ActionBaseFake(ArticleService, FieldService, ProductService, Logger, null);
		}

		[TestMethod]
		public void Process_ContentItemId_PassToProcessProduct()
		{
			Action.Process(Context);
			Assert.AreEqual<int>(Context.ContentItemIds[0], Action.LastProductId);
		}

		[TestMethod]
		public void Process_CatchException_LogException()
		{
			Action.ExceptionToThrow = new Exception();

			try
			{
				Action.Process(Context);
			}
			catch
			{
				Assert.AreEqual(Action.ExceptionToThrow, Logger.LastException);
			}
		}

		[TestMethod]
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