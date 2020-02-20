using System.Linq;
using NUnit.Framework;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestFixture]
    public class ArchiveActionTests : ActionTestsBase
    {
        [Test]
        public void ProcessProduct_NoRelations_Archive()
        {
            var productId = SetupNoRelation();
            Assert.AreEqual(1, Articles.Count());
            Assert.IsTrue(Articles.All(a => a.Id == productId && a.Archived));
        }

        [Test]
        public void ProcessProduct_M2ORelation_ArchiveReference()
        {
            var productId = SetupM2ORelation(null, DeletingMode.Delete);
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId));
            Assert.AreEqual(1, Articles.Count(a => a.Id != productId));
            Assert.IsTrue(Articles.All(a => a.Archived));
        }

        [Test]
        public void ProcessProduct_M2ORelation_IgnoreReference()
        {
            var productId = SetupM2ORelation(null, DeletingMode.Keep);
            Assert.AreEqual(2, Articles.Count());
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId && a.Archived));
            Assert.AreEqual(1, Articles.Count(a => a.Id != productId && !a.Archived));
        }

        [Test]
        public void ProcessProduct_M2ORelation_DefaultReference()
        {
            var productId = SetupM2ORelation(null, null);
            Assert.AreEqual(2, Articles.Count());
            Assert.AreEqual(1, Articles.Count(a => a.Id == productId && a.Archived));
            Assert.AreEqual(1, Articles.Count(a => a.Id != productId && !a.Archived));
        }

        protected override void InitializeAction()
        {
            Action = new ArchiveAction(ArticleService, FieldService, ProductService, CreateTransaction, NotificationService);
        }

        protected override void InitializeArticle(Quantumart.QP8.BLL.Article article)
        {
            article.Archived = false;
        }
    }
}