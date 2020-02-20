using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.DPC.Loader.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class ArticleDependencyTests
    {
        private static IArticleDependencyService _service;

        [OneTimeSetUp]
        public static void ClassInitialize()
        {
            var stopWatch = new Stopwatch();

            stopWatch.Start();
            _service = ObjectFactoryBase.Resolve<IArticleDependencyService>();
            stopWatch.Stop();
            Debug.WriteLine($"IArticleDependencyService отрезолвен за {stopWatch.Elapsed.TotalSeconds} сек");

            stopWatch.Reset();
            stopWatch.Start();
            ((ArticleDependencyService)_service).InitCache(false);
            stopWatch.Stop();
            Debug.WriteLine($"ArticleDependencyService инициализировал кеш {stopWatch.Elapsed.TotalSeconds} сек");
        }

        [Test]
        public void ExtensionFieldTest()
        {
            var fieldIds = new Dictionary<int, ChangedValue>
            {
                {1454, new ChangedValue {OldValue = "", NewValue = "2792"}}
            };

            Assert.IsTrue(_service.GetAffectedProducts(1566242, fieldIds).Any(x => x.Value.Contains(1565212)));
        }

        [Test]
        public void BackwardRelationOneToManyFieldTest()
        {
            Assert.IsTrue(_service.GetAffectedProducts(724341, new Dictionary<int, ChangedValue> { { 1472, new ChangedValue { OldValue = "", NewValue = "3093" } } }).Any(x => x.Value.Contains(699618)));
        }

        [Test]
        public void BackwardRelationOneToManySelfChangeFieldTest()
        {
            var productIds = _service.GetAffectedProducts(724341, new Dictionary<int, ChangedValue> { { 1471, new ChangedValue { OldValue = "699618", NewValue = "699611" } } });
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(699611)) && productIds.Any(x => x.Value.Contains(699618)));
        }

        [Test]
        public void BackwardRelationManyToManyFieldTest()
        {
            var productIds = _service.GetAffectedProducts(751938,
                new Dictionary<int, ChangedValue>
                {
                    {
                        1279,
                        new ChangedValue
                        {
                            OldValue = "847662",
                            NewValue = "1565235"
                        }
                    }
                });

            Assert.IsNotNull(productIds);
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(1565235)) && productIds.Any(x => x.Value.Contains(847662)));
        }

        [Test]
        public void EntityFieldOneToManyTest()
        {
            var productIds = _service.GetAffectedProducts(696525, new Dictionary<int, ChangedValue> { { 1112, new ChangedValue { OldValue = "", NewValue = "dfass" } } });
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(847666)));
        }

        [Test]
        public void EntityFieldManyToManyTest()
        {
            var productIds = _service.GetAffectedProducts(1762, new Dictionary<int, ChangedValue> { { 1137, new ChangedValue { OldValue = "sadf", NewValue = "sadfdf" } } });
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(1347778)));
        }

        [Test]
        public void EntityFieldManyToOneTest()
        {
            var productIds = _service.GetAffectedProducts(1347768, new Dictionary<int, ChangedValue> { { 1396, new ChangedValue { OldValue = "", NewValue = "2523" } } });
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(709179)));
        }

        [Test]
        public void LoadTest()
        {
            var productIds = _service.GetAffectedProducts(2242, new Dictionary<int, ChangedValue> { { 1120, new ChangedValue { OldValue = "", NewValue = "asd" } } });
            Debug.WriteLine($"Получено {productIds.SelectMany(x => x.Value).Count()} продуктов из {productIds.Count} контентов");
            Assert.IsTrue(productIds.Any(x => x.Value.Contains(1347778)));
        }
    }
}
