using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [Ignore]
    [TestClass]
    public class PublishActionTests : ActionTestsBase
    {
        private new QPNotificationServiceFake NotificationService { get; set; }

        private XmlProductServiceFake XmlProductService { get; set; }

        #region Tests methods
        [TestMethod]
        public void ProcessProduct_NoRelations_Archive()
        {
            var productId = SetupNoRelation();
            Assert.AreEqual(1, Articles.Count());
            Assert.IsTrue(Articles.All(a => a.Id == productId));
        }

        [TestMethod]
        public void ProcessProduct_M2ORelation_PublishReference()
        {
            var productId = SetupM2ORelation(null, DeletingMode.Delete);
            Assert.AreEqual(2, Articles.Count());
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
        }

        [TestMethod]
        public void ProcessProduct_M2ORelation_IgnoreReference()
        {
            var productId = SetupM2ORelation(null, DeletingMode.Keep);
            Assert.AreEqual(2, Articles.Count());
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
        }

        [TestMethod]
        public void ProcessProduct_M2ORelation_DefaultReference()
        {
            var productId = SetupM2ORelation(null, null);
            Assert.AreEqual(2, Articles.Count());
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
        }
        #endregion

        #region Overrides
        protected override void InitializeAction()
        {
            NotificationService = new QPNotificationServiceFake();
            XmlProductService = new XmlProductServiceFake();
            Action = new PublishAction(ArticleService, FieldService, ProductService, CreateTransaction, NotificationService, XmlProductService, FreezeService, ValidationService);
        }

        protected override Models.Entities.Article GetProductNoRelation(int productId)
        {
            var article = new Models.Entities.Article
            {
                Id = productId,
                Fields = new Dictionary<string, Models.Entities.ArticleField>()
            };

            return article;
        }

        protected override Models.Entities.Article GetProductM2ORelation(int productId)
        {
            var article = new Models.Entities.Article
            {
                Id = productId,
                Fields = new Dictionary<string, Models.Entities.ArticleField>()
            };

            return article;
        }
        #endregion
    }
}