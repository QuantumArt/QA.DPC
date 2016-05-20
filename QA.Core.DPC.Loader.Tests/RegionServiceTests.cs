using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Tests
{
    [TestClass]
    public class RegionServiceTests
    {
        [TestMethod]
        public void Test_get_parents()
        {
            var service = ObjectFactoryBase.Resolve<IRegionService>();

            var result = service.GetParentsIds(1945);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
        }
    }
}
