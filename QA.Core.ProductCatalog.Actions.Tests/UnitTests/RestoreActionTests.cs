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
	public class RestoreActionTests : ActionTestsBase
	{
		#region Test methods
		[TestMethod]
		public void ProcessProduct_NoRelations_Archive()
		{
			int productId = SetupNoRelation();
			Assert.AreEqual(1, Articles.Count());
			Assert.IsTrue(Articles.All(a => a.Id == productId && !a.Archived));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_ArchiveReference()
		{
			int productId = SetupM2ORelation(null, DeletingMode.Delete);
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
			Assert.AreEqual(1, Articles.Count(a => a.Id != productId));
			Assert.IsTrue(Articles.All(a => !a.Archived));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_IgnoreReference()
		{
			int productId = SetupM2ORelation(null, DeletingMode.Keep);
			Assert.AreEqual(2, Articles.Count());
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId && !a.Archived));
			Assert.AreEqual(1, Articles.Count(a => a.Id != productId && a.Archived));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_DefaultReference()
		{
			int productId = SetupM2ORelation(null, null);
			Assert.AreEqual(2, Articles.Count());
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId && !a.Archived));
			Assert.AreEqual(1, Articles.Count(a => a.Id != productId && a.Archived));
		}
		#endregion

		#region Overrides
		protected override void InitializeAction()
		{
			Action = new RestoreAction(ArticleService, FieldService, ProductService, Logger, CreateTransaction, NotificationService);
		}

		protected override void InitializeArticle(Quantumart.QP8.BLL.Article article)
		{
			article.Archived = true;
		}
		#endregion
	}
}