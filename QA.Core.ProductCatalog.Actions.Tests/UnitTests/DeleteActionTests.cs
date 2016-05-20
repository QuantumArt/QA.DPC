using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestClass]
	public class DeleteActionTests : ActionTestsBase
	{
		#region Test methods
		[TestMethod]
		public void ProcessProduct_NoRelations_Archive()
		{
			SetupNoRelation();
			Assert.IsFalse(Articles.Any());
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_DeleteReference()
		{
			SetupM2ORelation(null, DeletingMode.Delete);
			Assert.IsFalse(Articles.Any());			
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_IgnoreReference()
		{
			int productId = SetupM2ORelation(null, DeletingMode.Keep);
			Assert.AreEqual(1, Articles.Count());
			Assert.IsFalse(Articles.Any(a => a.Id == productId));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_DefaultReference()
		{
			int productId = SetupM2ORelation(null, null);
			Assert.AreEqual(1, Articles.Count());
			Assert.IsFalse(Articles.Any(a => a.Id == productId));
		}
		#endregion

		#region Overrides
		protected override void InitializeAction()
		{
			Action = new DeleteAction(ArticleService, FieldService, ProductService, Logger, CreateTransaction, NotificationService);
		}
		#endregion
	}
}