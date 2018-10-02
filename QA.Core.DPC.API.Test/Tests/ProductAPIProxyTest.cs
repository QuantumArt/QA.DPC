using System.Linq;
using System.Xaml;
using Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.API.Test.Tests
{
    [Ignore]
    [TestClass]
    public class ProductApiProxyTest : TestBase
    {
        #region Tests
        [TestMethod]
        public void ProductAPIProxy_GetProduct()
        {
            var service = Container.Resolve<IProductAPIService>();
            var originalProduct = GetProduct(ProductId);
            var apiProduct = service.GetProduct(Slug, Version, ProductId);
            var originalProductData = XamlServices.Save(originalProduct);
            var apiProductData = XamlServices.Save(apiProduct);
            Assert.AreEqual(originalProductData, apiProductData);
        }

        [TestMethod]
        public void ProductAPIProxy_GetNonExistingProduct_ReturnsNull()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = service.GetProduct(Slug, Version, 0);
            Assert.IsNull(product);
        }

        [TestMethod]
        public void ProductAPIProxy_GetProductList()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.GetProductsList(Slug, Version);
            Assert.IsTrue(products.Any(p => (int)p["id"] == ProductId));
        }

        [TestMethod]
        public void ProductAPIProxy_SearchProducts()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.SearchProducts(Slug, Version, "marketingproduct_producttype_serviceId=1f8e1077-bbfe-459a-88fc-705242b6408c");
            Assert.AreEqual(ProductId, products.Single());
        }

        [TestMethod]
        public void ProductAPIProxy_SearchMarketingProducts()
        {
            var service = Container.Resolve<IProductAPIService>();
            var products = service.SearchProducts(MarketingSlug, Version, "producttype_serviceId=1f8e1077-bbfe-459a-88fc-705242b6408c");
            Assert.AreEqual(MarketingProductId, products.Single());
        }

        [TestMethod]
        public void ProductAPIProxy_UpdateProduct()
        {
            var service = Container.Resolve<IProductAPIService>();
            var product = GetProduct(ProductId);
            service.UpdateProduct(Slug, Version, product);
        }

        [TestMethod]
        [ExpectedException(typeof(ActionException))]
        public void ProductAPIProxy_DeleteNonExistingProduct_ThrowsActionException()
        {
            var service = Container.Resolve<IProductAPIService>();

            try
            {
                service.CustomAction("DeleteAction", 0);
            }
            catch (ProxyExternalException ex)
            {
                throw ex.InnerException;
            }
        }
        #endregion
    }
}
