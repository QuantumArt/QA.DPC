using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests
{
    [Ignore]
    [TestClass]
    public class FakeProductLoaderTests
    {
        [TestMethod]
        public void Test_That_Fake_Service_Works()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var product = service.GetProductById(2360);
            Assert.IsNotNull(product);
        }
    }
}
