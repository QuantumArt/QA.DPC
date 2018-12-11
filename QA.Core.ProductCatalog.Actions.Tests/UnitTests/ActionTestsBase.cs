using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.Logger;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.Actions.Tests.Fakes;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.UnitTests
{
    public abstract class ActionTestsBase
    {
        #region Constants
        private const int ContentId = 1;
        #endregion

        #region Protected properties
        protected ArticleServiceFake ArticleService { get; set; }
        protected FieldServiceFake FieldService { get; set; }
        protected ProductServiceFake ProductService { get; set; }
        protected QPNotificationServiceFake NotificationService { get; set; }
        protected ILogger Logger { get; set; }
        protected TransactionFake Transaction { get; set; }
        protected IAction Action { get; set; }
        protected ActionContext Context { get; set; }
        protected IXmlProductService XmlService { get; private set; }
        protected IFreezeService FreezeService { get; set; }
        protected IValidationService ValidationService { get; set; }
        protected IEnumerable<Quantumart.QP8.BLL.Article> Articles
        {
            get { return ArticleService.Articles.Values; }
        }
        #endregion

        #region Initialization
        [TestInitialize]
        public void Initialize()
        {
            ArticleService = new ArticleServiceFake();
            NotificationService = new QPNotificationServiceFake();
            FieldService = new FieldServiceFake();
            FreezeService = new FreezeServiceFake();
            ValidationService = new ValidationServiceFake();
            ProductService = new ProductServiceFake();
            ProductService.Content = new Content();
            Transaction = null;
            Logger = new LoggerFake();
            Context = new ActionContext();
            InitializeAction();
        }
        #endregion

        #region Test methods
        [TestMethod]
        [ExpectedException(typeof(ActionException))]
        public void ProcessProduct_ArticleNotExists_ThrowException()
        {
            Context.ContentItemIds = new[] { 0 };
            Action.Process(Context);
        }
        #endregion

        #region Abstract and virtual methods
        protected abstract void InitializeAction();

        protected virtual void InitializeArticle(Quantumart.QP8.BLL.Article article)
        {
        }

        protected virtual Models.Entities.Article GetProductNoRelation(int productId)
        {
            return new Models.Entities.Article();
        }

        protected virtual Models.Entities.Article GetProductM2ORelation(int productId)
        {
            return new Models.Entities.Article();
        }
        #endregion

        #region Protected methods
        protected ITransaction CreateTransaction()
        {
            if (Transaction == null)
            {
                Transaction = new TransactionFake();
            }
            else
            {
                Assert.Fail("Transaction factory is called more than once");
            }

            return Transaction;
        }

        protected int SetupNoRelation()
        {
            var article = ArticleService.New(ContentId);
            InitializeArticle(article);
            ArticleService.Save(article);
            Context.ContentItemIds = new[] { article.Id };
            ProductService.Article = GetProductNoRelation(article.Id);

            ArticleService.ArticleSaved += (s, e) => VerifyArticleOnSave(e.Article);
            Action.Process(Context);

            VerifyTransaction();

            return article.Id;
        }

        protected int SetupM2ORelation(CloningMode? cloningMode, DeletingMode? deletingMode)
        {
            if (cloningMode != null || deletingMode != null)
            {
                var field = new EntityField();
                field.FieldId = 1;
                field.CloningMode = cloningMode == null ? default(CloningMode) : cloningMode.Value;
                field.DeletingMode = deletingMode == null ? default(DeletingMode) : deletingMode.Value;
                ProductService.Content.Fields.Add(field);
            }

            var referencedArticle = ArticleService.New(ContentId);
            InitializeArticle(referencedArticle);
            ArticleService.Save(referencedArticle);

            var rootArticle = ArticleService.New(ContentId);
            InitializeArticle(rootArticle);
            var fv = new Quantumart.QP8.BLL.FieldValue();
            fv.Article = rootArticle;
            fv.Field = new Quantumart.QP8.BLL.Field();
            fv.Field.Id = 1;
            fv.Field.Init();
            fv.Field.ExactType = Quantumart.QP8.Constants.FieldExactTypes.M2ORelation;
            fv.UpdateValue(referencedArticle.Id.ToString());
            rootArticle.FieldValues.Add(fv);
            ArticleService.Save(rootArticle);
            Context.ContentItemIds = new[] { rootArticle.Id };
            ProductService.Article = GetProductM2ORelation(rootArticle.Id);

            ArticleService.ArticleSaved += (s, e) => VerifyArticleOnSave(e.Article);
            Action.Process(Context);

            VerifyTransaction();

            return rootArticle.Id;
        }
        #endregion

        #region Private methods
        private void VerifyTransaction()
        {
            Assert.IsNotNull(Transaction, "Transaction is not used");
            Assert.IsTrue(Transaction.IsCommited, "Transaction is not commited");
            Assert.IsTrue(Transaction.IsDispossed, "Transaction is not dispossed");
        }

        private void VerifyArticleOnSave(Quantumart.QP8.BLL.Article article)
        {
            Assert.IsNotNull(Transaction, "Article " + article.Id + " saved not in transaction");
            Assert.IsFalse(Transaction.IsCommited, "Article " + article.Id + " saved after transaction been committed");
            Assert.IsFalse(Transaction.IsDispossed, "Article " + article.Id + " saved out of transaction scope");
        }
        #endregion
    }
}
