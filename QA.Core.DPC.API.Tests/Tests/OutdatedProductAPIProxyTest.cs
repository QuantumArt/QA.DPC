using System.Net.Http;
using Moq;
using NUnit.Framework;
using QA.Core.DPC.API.Container;
using QA.Core.DPC.API.Update;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using Unity;

namespace QA.Core.DPC.API.Test.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class OutdatedProductAPIProxyTest : TestBase
    {
        private const int Id = 2225400;
        private const string Slug = "Q";
        private const string Version = "v1";
        [Test]
        public void Test_Proxy_Read()
        {
            var service = Container.Resolve<IProductAPIService>();
            var roduct = service.GetProduct(Slug, Version, Id);
            Assert.AreEqual(Id, roduct.Id);
        }

        [Test]
        public void Test_Proxy_Update_Actual()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = service.GetProduct(Slug, Version, Id);
            Assert.AreEqual(Id, product.Id);
            Update(product);
            service.UpdateProduct(Slug, Version, product);
        }

        [Test]
        public void Test_Proxy_Update_Outdated()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = service.GetProduct(Slug, Version, Id);
            Assert.AreEqual(Id, product.Id);
            Update(product);
            service.UpdateProduct(Slug, Version, product);
            Update(product);

            try
            {
                service.UpdateProduct(Slug, Version, product);
                Assert.Fail("ProductUpdateConcurrencyException expected");
            }
            catch (ProxyExternalException ex)
            {
                var uex = ex.InnerException as ProductUpdateConcurrencyException;
                Assert.IsNotNull(uex);
                CollectionAssert.AreEquivalent(new[] { Id }, uex.ArticleIds);
            }
        }

        private void Update(Article product)
        {
            var field = (PlainArticleField)product.Fields["Question"];
            field.NativeValue += "*";
            field.Value += "*";
        }
    }
}
