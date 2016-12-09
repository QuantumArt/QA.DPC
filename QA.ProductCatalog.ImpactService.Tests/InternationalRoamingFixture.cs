using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace QA.ProductCatalog.ImpactService.Tests
{
    [TestFixture]
    public class InternationalRoamingFixture
    {
        [OneTimeSetUp]
        public void Start()
        {
            

        }

        private JObject GetJsonFromFile(string file)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TestData\\{file}");
            return JObject.Parse(File.ReadAllText(path));
        }

        [Test]
        public void TestBasicImpact()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            calculator.Calculate(tariff, option);
            var root = tariff.SelectToken("product.Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(45));
            Assert.That(result[0]["Title"], Is.Not.EqualTo("Новый заголовок"));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("product.Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }

        [Test]
        public void TestBasicWithoutImpact()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("product.Parameters");
            calculator.FindByKey(optionRoot, direction.GetKey()).First()["NumValue"] = 95;
            calculator.Calculate(tariff, option);
            var root = tariff.SelectToken("product.Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(result[0]["Changed"], Is.Null);
            Assert.That(tariff.SelectTokens("product.Parameters.[?(@.Changed)]").Count(), Is.EqualTo(0));
        }

        [Test]
        public void TestFindByKey()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var root = tariff.SelectToken("product.Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var keys = root.SelectTokens("[?(@.BaseParameter)]").Select(n => n.ExtractDirection().GetKey());
            var result = new InternationalRoamingCalculator().FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
        }


    }
}
