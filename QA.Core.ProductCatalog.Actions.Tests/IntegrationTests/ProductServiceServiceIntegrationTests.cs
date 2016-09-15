using System;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [Ignore]
    [TestClass]
    public class ProductServiceIntegrationTests : IntegrationTestsBase
    {
        private const int ProductId = 2360;
        private const int MarketingProductId = 2423;


        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_ProductService_GetProductDefinition()
        {
            CastomAction_ProductService_GetProductDefinition(ProductId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_ProductService_GetMarketingProductDefinition()
        {
            CastomAction_ProductService_GetProductDefinition(MarketingProductId);
        }


        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_ProductService_GetProductById()
        {
            CastomAction_ProductService_GetProductById(ProductId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_ProductService_GetMarketingProductById()
        {
            CastomAction_ProductService_GetProductById(MarketingProductId);
        }

        private void CastomAction_ProductService_GetProductDefinition(int productId)
        {
            var articleService = Container.Resolve<IArticleService>();
            var productService = Container.Resolve<IProductService>();
            var createTransaction = Container.Resolve<Func<ITransaction>>();

            using (var transaction = createTransaction())
            {
                var product = articleService.Read(productId);
                productService.GetProductDefinition(1, product.ContentId);
                transaction.Commit();
            }
        }

        public void CastomAction_ProductService_GetProductById(int productId)
        {
            var productService = Container.Resolve<IProductService>();
            var createTransaction = Container.Resolve<Func<ITransaction>>();

            using (var transaction = createTransaction())
            {
                productService.GetProductById(productId);
                transaction.Commit();
            }
        }
    }
}