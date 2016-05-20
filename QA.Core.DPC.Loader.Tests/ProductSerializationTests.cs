using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QA.Configuration;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader.Tests
{
    [TestClass]
    public class ProductSerializationTests
    {

        [TestMethod]
        public void TestXmlDeserialization()
        {
            var xmlProductService = ObjectFactoryBase.Resolve<IXmlProductService>();

            var xDoc = XDocument.Load("TestData\\Product.xml");

            string defStr = File.ReadAllText("TestData\\ProductDefinition.xaml");

            Article resultArticle = xmlProductService.DeserializeProductXml(xDoc, (Content)XamlConfigurationParser.CreateFrom(defStr));

            var st = new Stopwatch();

            st.Start();

            var art2 = xmlProductService.DeserializeProductXml(xDoc, (Content)XamlConfigurationParser.CreateFrom(defStr));

            st.Stop();

            Debug.WriteLine(st.Elapsed);

            string resultXaml = XamlConfigurationParser.CreateFromObject(resultArticle);

            string referenceXaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            //Assert.AreEqual(resultXaml, referenceXaml);
        }

        [TestMethod]
        public void TextJsonDeserialization()
        {
            var jsonProductService = ObjectFactoryBase.Resolve<IJsonProductService>();

            string defStr = File.ReadAllText("TestData\\ProductDefinition.xaml");

            string json = File.ReadAllText("TestData\\ProductJson.js");

            Article resultArticle = jsonProductService.DeserializeProduct(json, (Content)XamlConfigurationParser.CreateFrom(defStr));

            string resultXaml = XamlConfigurationParser.CreateFromObject(resultArticle);

            string referenceXaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            Assert.AreEqual(resultXaml, referenceXaml);
        }

        [TestMethod]
        public void TestEvaluatorExtensionField()
        {
            string xaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);

            var res = FilterableBindingValueProvider.EvaluatePath("MarketingProduct[ProductType='289']", productDto);

            Assert.IsTrue(res.Length>0);
        }

        [TestMethod]
        public void TestEvaluatorRootFilter()
        {
            string xaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);

            var res = FilterableBindingValueProvider.EvaluatePath("[Type='305']", productDto);

            Assert.IsTrue(res.Length > 0);
        }


        


        [TestMethod]
        public void Temp()
        {
            var productService = ObjectFactoryBase.Resolve<IProductService>();

            var article = productService.GetProductById(2166354);

            string resultXaml = XamlConfigurationParser.CreateFromObject(article);
        }

    }
}
