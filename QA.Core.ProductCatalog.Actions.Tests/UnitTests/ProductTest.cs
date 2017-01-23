using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using Quantumart.QP8.BLL;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    [TestClass]
    public class ProductTest
    {
        #region Constants
        private const int FieldId = 10;
        private const int MissedFieldId = 11;
        private const CloningMode Mode = CloningMode.Ignore;
        #endregion

        #region Private fields
        private Article Article { get; set; }
        private Dictionary<int, CloningMode> FieldModes { get; set; }
        public List<FieldValue> BackwardFieldValues { get; set; }
        private Quantumart.QP8.BLL.Field Field { get; set; }
        #endregion

        #region Initialization
        [TestInitialize]
        public void Initialize()
        {
            Article = (Article)Activator.CreateInstance(typeof(Article), true);
            FieldModes = new Dictionary<int, CloningMode>();
            FieldModes.Add(FieldId, Mode);
            Article.FieldValues = new List<FieldValue>();
            BackwardFieldValues = new List<FieldValue>();
            Field = new Quantumart.QP8.BLL.Field();
        }
        #endregion

        #region Test methods
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_ArticleIsNull_ThrowException()
        {
            var product = new Product<CloningMode>(null, FieldModes, BackwardFieldValues);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_FieldModesIsNull_ThrowException()
        {
            var product = new Product<CloningMode>(Article, null, BackwardFieldValues);
        }

        [Ignore]
        [TestMethod]
        public void Article_ArticleField_IsInitialized()
        {
            var product = new Product<CloningMode>(Article, FieldModes, BackwardFieldValues);
            Assert.AreEqual(Article, product.Article);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCloningMode_NullArgument_ThrowException()
        {
            var product = new Product<CloningMode>(Article, FieldModes, BackwardFieldValues);
            product.GetCloningMode(null);
        }

        [Ignore]
        [TestMethod]
        public void GetCloningMode_MissedFieldArgument_ReturnsNull()
        {
            Field.Id = MissedFieldId;
            var product = new Product<CloningMode>(Article, FieldModes, BackwardFieldValues);
            var mode = product.GetCloningMode(Field);
            Assert.IsNull(mode);
        }

        [Ignore]
        [TestMethod]
        public void GetCloningMode_ExistingField_ReturnsCloningMode()
        {
            Field.Id = FieldId;
            var product = new Product<CloningMode>(Article, FieldModes, BackwardFieldValues);
            var mode = product.GetCloningMode(Field);
            Assert.AreEqual(Mode, mode);
        }
        #endregion
    }
}
