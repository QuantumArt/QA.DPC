using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
	[TestClass]
	public class PublishActionTests : ActionTestsBase
	{
		private QPNotificationServiceFake NotificationService { get; set; }
		private XmlProductServiceFake XmlProductService { get; set; }

		#region Tests methods
		[TestMethod]
		public void ProcessProduct_NoRelations_Archive()
		{
			int productId = SetupNoRelation();
			Assert.AreEqual(1, Articles.Count());
			Assert.IsTrue(Articles.All(a => a.Id == productId));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_PublishReference()
		{
			int productId = SetupM2ORelation(null, DeletingMode.Delete);
			Assert.AreEqual(2, Articles.Count());
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_IgnoreReference()
		{
			int productId = SetupM2ORelation(null, DeletingMode.Keep);
			Assert.AreEqual(2, Articles.Count());
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
		}

		[TestMethod]
		public void ProcessProduct_M2ORelation_DefaultReference()
		{
			int productId = SetupM2ORelation(null, null);
			Assert.AreEqual(2, Articles.Count());
			Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
		}
		#endregion

		#region Overrides
		protected override void InitializeAction()
		{
			NotificationService = new QPNotificationServiceFake();
			XmlProductService = new XmlProductServiceFake();
			Action = new PublishAction(ArticleService, FieldService, ProductService, Logger, CreateTransaction, NotificationService, XmlProductService, FreezeService);
		}

		protected override Models.Entities.Article GetProductNoRelation(int productId)
		{
			var article = new Models.Entities.Article();
			article.Id = productId;
			article.Fields = new Dictionary<string, Models.Entities.ArticleField>();
			return article;
		}

		protected override Models.Entities.Article GetProductM2ORelation(int productId)
		{
			var article = new Models.Entities.Article();
			article.Id = productId;
			article.Fields = new Dictionary<string, Models.Entities.ArticleField>();
			return article;
		}
		#endregion
	}
}