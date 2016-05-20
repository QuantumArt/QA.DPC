using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Core.DPC.Loader;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests
{
    [TestClass]
    public class FakeProductLoaderTests
    {
        [TestMethod]
        public void Test_That_Fake_Service_Works()
        {
            //var service =  new FakeProductLoader(new ContentDefinitionService());
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var product = service.GetProductById(2360);

            Assert.IsNotNull(product);
        }
    }
}
