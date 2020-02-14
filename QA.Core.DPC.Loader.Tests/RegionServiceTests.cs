using NUnit.Framework;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class RegionServiceTests
    {
        [Test]
        public void Test_get_parents()
        {
            var service = ObjectFactoryBase.Resolve<IRegionService>();
            var result = service.GetParentsIds(1945);
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }
    }
}
