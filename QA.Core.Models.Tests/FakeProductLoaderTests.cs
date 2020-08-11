using NUnit.Framework;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.Models.Tests
{
    [TestFixture]
    public class FakeProductLoaderTests
    {
        [Test]
        //TODO fix portable.xaml
      //  [Ignore("Manual")]
        public void Test_That_Fake_Service_Works()
        {
            var service = ObjectFactoryBase.Resolve<IProductService>();
            var product = service.GetProductById(2360);
            Assert.IsNotNull(product);
        }
    }
}
