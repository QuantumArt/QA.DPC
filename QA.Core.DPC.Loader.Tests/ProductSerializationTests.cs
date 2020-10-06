using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using QA.Configuration;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.Processors;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Tests
{
    [Ignore("Manual")]
    [TestFixture]
    public class ProductSerializationTests
    {

        [Test]
        public void TestXmlDeserialization()
        {
            var xmlProductService = ObjectFactoryBase.Resolve<IXmlProductService>();
            var xDoc = XDocument.Load("TestData\\Product.xml");
            var defStr = File.ReadAllText("TestData\\ProductDefinition.xaml");
            var resultArticle = xmlProductService.DeserializeProductXml(xDoc, (Content)XamlConfigurationParser.CreateFrom(defStr));
            var st = new Stopwatch();

            st.Start();
            xmlProductService.DeserializeProductXml(xDoc, (Content)XamlConfigurationParser.CreateFrom(defStr));
            st.Stop();

            Debug.WriteLine(st.Elapsed);
            XamlConfigurationParser.Save(resultArticle);
            File.ReadAllText("TestData\\ReferenceDto.xaml");
        }

        [Test]
        public void TextJsonDeserialization()
        {
            var jsonProductService = ObjectFactoryBase.Resolve<IJsonProductService>();
            var defStr = File.ReadAllText("TestData\\ProductDefinition.xaml");
            var json = File.ReadAllText("TestData\\ProductJson.js");
            var resultArticle = jsonProductService.DeserializeProduct(json, (Content)XamlConfigurationParser.CreateFrom(defStr));
            var resultXaml = XamlConfigurationParser.Save(resultArticle);
            var referenceXaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            Assert.AreEqual(resultXaml, referenceXaml);
        }

        [Test]
        public void TestEvaluatorExtensionField()
        {
            var xaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);
            var res = DPathProcessor.Process("MarketingProduct[ProductType='289']", productDto);
            Assert.IsTrue(res.Length > 0);
        }

        [Test]
        public void TestEvaluatorRootFilter()
        {
            var xaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);
            var res = DPathProcessor.Process("[Type='305']", productDto);
            Assert.IsTrue(res.Length > 0);
        }

        [Test]
        public void Temp()
        {
            var productService = ObjectFactoryBase.Resolve<IProductService>();
            var article = productService.GetProductById(2166354);
            XamlConfigurationParser.Save(article);
        }
    }
}
