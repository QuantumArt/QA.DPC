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
    public class ArticleEvalTests
    {
        [TestMethod]
        public void TestEvaluatorExtensionField()
        {
            var productDto = GetProductDto();

            var res = FilterableBindingValueProvider.EvaluatePath("MarketingProduct[ProductType='289']", productDto);

            Assert.IsTrue(res.Length>0);
        }

        private static Article GetProductDto()
        {
            string xaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            return (Article) XamlConfigurationParser.CreateFrom(xaml);
        }

        [TestMethod]
        public void TestEvaluatorRootFilter()
        {
            string xaml = File.ReadAllText("TestData\\ReferenceDto.xaml", Encoding.GetEncoding("windows-1251"));

            var productDto = (Article)XamlConfigurationParser.CreateFrom(xaml);

            var res = FilterableBindingValueProvider.EvaluatePath("[Type='305']", productDto);

            Assert.IsTrue(res.Length > 0);
        }

    }
}
