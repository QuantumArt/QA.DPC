using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Configuration;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.Processors;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Tests
{
    [TestClass]
    public class ProductSerializationTests
    {
        [Ignore]
        [TestMethod]
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
            XamlConfigurationParser.CreateFromObject(resultArticle);
            File.ReadAllText("TestData\\ReferenceDto.xaml");
        }

        [Ignore]
        [TestMethod]
        public void TextJsonDeserialization()
        {
            var jsonProductService = ObjectFactoryBase.Resolve<IJsonProductService>();
            var defStr = File.ReadAllText("TestData\\ProductDefinition.xaml");
            var json = File.ReadAllText("TestData\\ProductJson.js");
            var resultArticle = jsonProductService.DeserializeProduct(json, (Content)XamlConfigurationParser.CreateFrom(defStr));
            var resultXaml = XamlConfigurationParser.CreateFromObject(resultArticle);
            var referenceXaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            Assert.AreEqual(resultXaml, referenceXaml);
        }

        [TestMethod]
        public void TestEvaluatorExtensionField()
        {
            var xaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);
            var res = DPathProcessor.Process("MarketingProduct[ProductType='289']", productDto);
            Assert.IsTrue(res.Length > 0);
        }

        [TestMethod]
        public void TestEvaluatorRootFilter()
        {
            var xaml = File.ReadAllText("TestData\\ReferenceDto.xaml");
            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);
            var res = DPathProcessor.Process("[Type='305']", productDto);
            Assert.IsTrue(res.Length > 0);
        }

        [Ignore]
        [TestMethod]
        public void Temp()
        {
            var productService = ObjectFactoryBase.Resolve<IProductService>();
            var article = productService.GetProductById(2166354);
            XamlConfigurationParser.CreateFromObject(article);
        }
    }
}
