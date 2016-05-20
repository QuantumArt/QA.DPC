﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.Unity;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;

using QA.Core.ProductCatalog.Actions.Container;
using System.Transactions;
using Quantumart.QP8.BLL;
using System.Threading.Tasks;
using QA.Core.Models;

namespace QA.Core.ProductCatalog.Actions.Tests.IntegrationTests
{
    [TestClass]
    public class XmlProductServiceIntegrationTests : IntegrationTestsBase
    {
        private const int ProductId = 2405;
        private const int MarketingProductId = 2403;


        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_XmlProductService_GetXmlForProduct()
        {
            CastomAction_XmlProductService_GetXmlForProducts(ProductId);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_XmlProductService_GetXmlForProduct_serializable()
        {
            CastomAction_XmlProductService_GetXmlForProducts_transaction(695074, IsolationLevel.Serializable);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_XmlProductService_GetXmlForProduct_ReadUncommitted()
        {
            CastomAction_XmlProductService_GetXmlForProducts_transaction(695074, IsolationLevel.ReadUncommitted);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CastomAction_XmlProductService_GetXmlForMarketingProduct()
        {
            CastomAction_XmlProductService_GetXmlForProducts(MarketingProductId);
        }


        public void CastomAction_XmlProductService_GetXmlForProducts(int productId)
        {
            var productService = Container.Resolve<IProductService>();
            var xmlProductService = Container.Resolve<IXmlProductService>();
            var createTransaction = Container.Resolve<Func<ITransaction>>();

            using (var transaction = createTransaction())
            {
                var product = productService.GetProductById(productId);
                xmlProductService.GetProductXml(product, ArticleFilter.DefaultFilter);
                transaction.Commit();
            }
        }


        public void CastomAction_XmlProductService_GetXmlForProducts_transaction(int productId, IsolationLevel il)
        {
            var productService = Container.Resolve<IProductService>();
            var xmlProductService = Container.Resolve<IXmlProductService>();

            using (var cs = new QPConnectionScope(Container.GetConnectionString()))
            {
                using (var ts = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { Timeout = TimeSpan.FromMinutes(2), IsolationLevel = il }))
                {
                    var product = productService.GetProductById(productId);
                    xmlProductService.GetProductXml(product, ArticleFilter.DefaultFilter);
                    Do(100);
                    ts.Complete();
                }
            }
        }

        private void Do(int ms) {
            Task.WaitAll(Task.Run(async () => { await DoFakeWork(ms); }), DoFakeWork(300));
        }

        private Task DoFakeWork(int ms)
        {
            return Task.Delay(ms);
        }
    }
}