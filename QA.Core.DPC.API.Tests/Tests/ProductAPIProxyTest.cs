using System.Linq;
using Portable.Xaml;
using Unity;
using NUnit.Framework;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.API.Test.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class ProductApiProxyTest : TestBase
    {
        #region Tests
        [Test]
        public void ProductAPIProxy_GetProduct()
        {
            var service = Container.Resolve<IProductAPIService>();
            var originalProduct = GetProduct(ProductId);
            var apiProduct = service.GetProduct(Slug, Version, ProductId);
            var originalProductData = XamlServices.Save(originalProduct);
            var apiProductData = XamlServices.Save(apiProduct);
            Assert.AreEqual(originalProductData, apiProductData);
        }

        [Test]
        public void ProductAPIProxy_GetNonExistingProduct_ReturnsNull()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = service.GetProduct(Slug, Version, 0);
            Assert.IsNull(product);
        }

        [Test]
        public void ProductAPIProxy_GetProductList()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.GetProductsList(Slug, Version);
            Assert.IsTrue(products.Any(p => (int)p["id"] == ProductId));
        }

        [Test]
        public void ProductAPIProxy_SearchProducts()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.SearchProducts(Slug, Version, "marketingproduct_producttype_serviceId=1f8e1077-bbfe-459a-88fc-705242b6408c");
            Assert.AreEqual(ProductId, products.Single());
        }

        [Test]
        public void ProductAPIProxy_SearchMarketingProducts()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.SearchProducts(MarketingSlug, Version, "producttype_serviceId=1f8e1077-bbfe-459a-88fc-705242b6408c");
            Assert.AreEqual(MarketingProductId, products.Single());
        }

        [Test]
        public void ProductAPIProxy_UpdateProduct()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = GetProduct(ProductId);
            service.UpdateProduct(Slug, Version, product);
        }
        
        [Test]
        public void ProductAPIProxy_DeleteNonExistingProduct_ThrowsActionException()
        {
            var service = Container.Resolve<IProductAPIService>();
            Assert.That(() =>
            {
                try
                {
                    service.CustomAction("DeleteAction", 0);
                }
                catch (ProxyExternalException ex)
                {
                    throw ex.InnerException;
                }
            }, NUnit.Framework.Throws.Exception.TypeOf<ActionException>());
        }
        #endregion
    }
}
