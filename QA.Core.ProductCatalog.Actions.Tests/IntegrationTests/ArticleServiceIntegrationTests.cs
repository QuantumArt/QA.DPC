using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
	[TestClass]
	public class ArticleServiceIntegrationTests : IntegrationTestsBase
	{
		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_ArticleService_Read()
		{
			var service = Container.Resolve<IArticleService>();
			var createTransaction = Container.Resolve<Func<ITransaction>>();

			using (var transaction = createTransaction())
			{
				var a1 = service.Read(0);
				var a2 = service.Read(2360);

				Assert.IsNull(a1);
				Assert.IsNotNull(a2);
				Assert.IsNotNull(a2.FieldValues);

				transaction.Commit();				
			}
		}

		[TestMethod]
		[TestCategory("Integration")]
		public void CastomAction_ArticleService_Save()
		{
			var service = Container.Resolve<IArticleService>();
			var createTransaction = Container.Resolve<Func<ITransaction>>();

			using (var transaction = createTransaction())
			{
				var a = service.Read(2360);
				a = service.Save(a);
				transaction.Commit();

				Assert.IsNotNull(a);				
			}
		}	
	}
}