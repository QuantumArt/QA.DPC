﻿using System.Linq;
using NUnit.Framework;
using QA.Core.Models.Configuration;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestFixture]
    public class DeleteActionTests : ActionTestsBase
    {
        #region Test methods
        [Test]
        public void ProcessProduct_NoRelations_Archive()
        {
            SetupNoRelation();
            Assert.IsFalse(Articles.Any());
        }

        [Test]
        public void ProcessProduct_M2ORelation_DeleteReference()
        {
            SetupM2ORelation(null, DeletingMode.Delete);
            Assert.IsFalse(Articles.Any());
        }

        [Test]
        public void ProcessProduct_M2ORelation_IgnoreReference()
        {
            var productId = SetupM2ORelation(null, DeletingMode.Keep);
            Assert.AreEqual(1, Articles.Count());
            Assert.IsFalse(Articles.Any(a => a.Id == productId));
        }

        [Test]
        public void ProcessProduct_M2ORelation_DefaultReference()
        {
            var productId = SetupM2ORelation(null, null);
            Assert.AreEqual(1, Articles.Count());
            Assert.IsFalse(Articles.Any(a => a.Id == productId));
        }
        #endregion

        #region Overrides
        protected override void InitializeAction()
        {
            Action = new DeleteAction(ArticleService, FieldService, ProductService, CreateTransaction, NotificationService);
        }
        #endregion
    }
}