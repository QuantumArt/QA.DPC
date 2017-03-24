using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace QA.ProductCatalog.ImpactService.Tests
{
    [TestFixture]
    public class BaseImpactCalculatorFixture
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
        public void ChangeParameters_MixedOrder_Reordered()
        {
            var tariff = GetJsonFromFile("simple_tariff_order.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, new JObject[] {}, null);

            var orders = new[] {34, 33, 32, 1000, 11, 12, 2000, 21, 22, 1};

            var resultOrders = tariff.SelectTokens("Parameters.[?(@.Id)].Id").Select(n => (int) n).ToArray();

            Assert.That(resultOrders, Is.EqualTo(orders));

        }

        [Test]
        public void ChangeParameters_NumValueSmaller_Changed()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(45));
            Assert.That((decimal)result[0]["OldNumValue"], Is.EqualTo(85));
            Assert.That((string)result[0]["Title"], Is.Not.EqualTo("Новый заголовок"));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }


        [Test]
        public void ChangeParameters_NumValueSmallerWithLinkMerging_Changed()
        {
            var tariff = GetJsonFromFile("simple2_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(65));
            Assert.That((string)result[0]["Title"], Is.Not.EqualTo("Новый заголовок"));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ChangeParameters_NumValueSmallerWithoutCalculateModifier_NotChanged()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var parameter = calculator.FindByKey(optionRoot, direction.GetKey()).First();
            var modifier = parameter.SelectToken($"Modifiers.[?(@.Alias == '{calculator.ParameterModifierName}')]");
            modifier.Remove();
            
            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(result[0]["Changed"], Is.Null);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(0));
        }

        [Test]
        public void ChangeParameters_NumValueSmallerWithoutOptionCalculateModifier_NotChanged()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var optionId = (decimal)option.SelectToken("Id");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var modifier = tariff.SelectTokens($"{calculator.LinkName}.[?(@.Service)]")
                .Single(n => (decimal)n["Service"]["Id"] == optionId)
                .SelectToken($"Parent.Modifiers.[?(@.Alias == '{calculator.LinkModifierName}')]");
            modifier.Remove();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(result[0]["Changed"], Is.Null);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(0));
        }


        [Test]
        public void ChangeParameters_NumValueGreater_NotChanged()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            calculator.FindByKey(optionRoot, direction.GetKey()).First()["NumValue"] = 95;

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(result[0]["Changed"], Is.Null);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(0));
        }

        [Test]
        public void ChangeParameters_NumValueGreaterWithForcedInfluence_Changed()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "ForcedInfluence",
                ["Title"] = "Принудительное влияние"

            };
            var parameter = calculator.FindByKey(optionRoot, direction.GetKey()).First();
            ((JArray)parameter.SelectToken("Modifiers")).Add(obj);
            parameter["NumValue"] = 95;

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(95));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ProcessRemove_HasRemoveModifier_Removed()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Remove",
                ["Title"] = "Удалить"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(0));
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Id)]").Count(), Is.GreaterThan(0));
        }

        [Test]
        public void ProcessRemove_HasRemoveModifierWithLinkMerging_Removed()
        {
            var tariff = GetJsonFromFile("simple2_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = tariff.SelectTokens($"{calculator.LinkName}.[?(@.Id)].Parent.Parameters").First();
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Remove",
                ["Title"] = "Удалить"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);
            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(0));
            Assert.That(cntAfter, Is.EqualTo(cntBefore - 1));
        }

        [Test]
        public void ProcessAppend_HasAppendModifier_Appended()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Append",
                ["Title"] = "Добавить"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);
            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);
            calculator.Reorder(tariff);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(result[0].Previous["Changed"], Is.Not.Null);

            Assert.That(cntAfter, Is.EqualTo(cntBefore + 1));
        }

        [Test]
        public void ProcessAppend_HasAppendModifierWithLinkMerging_Appended()
        {
            var tariff = GetJsonFromFile("simple2_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = tariff.SelectTokens($"{calculator.LinkName}.[?(@.Id)].Parent.Parameters").First();
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Append",
                ["Title"] = "Добавить"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);
            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That(cntAfter, Is.EqualTo(cntBefore + 1));
        }




        [Test]
        public void ProcessAppend_HasAppendOrReplaceModifierAndDirectionDoesntExist_Appended()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "AppendOrReplace",
                ["Title"] = "Добавить"

            };

            var obj2 = new JObject
            {
                ["Id"] = 1100,
                ["Alias"] = "RussiaExceptHome",
                ["Title"] = "Россия кроме домашнего региона"

            };

            var parameter = calculator.FindByKey(optionRoot, direction.GetKey()).First();
            ((JArray)parameter.SelectToken("Modifiers")).Add(obj);
            parameter["Zone"] = obj2;

            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            var result2 = tariff.SelectToken("Parameters.[?(@.Id==3000)]");

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(cntAfter, Is.EqualTo(cntBefore + 1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That((decimal)result2["NumValue"], Is.EqualTo(45));
            Assert.That(result[0]["Changed"], Is.Null);
            Assert.That((bool)result2["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }

        [Test]
        public void ProcessAppend_HasAppendOrReplaceModifierAndDirectionExists_Changed()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "AppendOrReplace",
                ["Title"] = "Добавить"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);
            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(cntAfter, Is.EqualTo(cntBefore));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(45));
            Assert.That((string)result[0]["Title"], Is.Not.EqualTo("Новый заголовок"));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(1));
        }


        [Test]
        public void ChangeParameters_HasAddModifier_Added()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Add",
                ["Title"] = "Cкладывать значения"

            };
            ((JArray)calculator.FindByKey(optionRoot, direction.GetKey()).First().SelectToken("Modifiers")).Add(obj);
            var cntBefore = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();

            calculator.Calculate(tariff, option);

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(cntAfter, Is.EqualTo(cntBefore));
            Assert.That((decimal) result[0]["NumValue"], Is.EqualTo(130));
            Assert.That((string)result[0]["Title"], Is.Not.EqualTo("Новый заголовок"));
            Assert.That((bool)result[0]["Changed"], Is.True);
        }

        [Test]
        public void ChangeParameters_HasDiscountModifier_DiscountApplied()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var optionRoot = option.SelectToken("Parameters");
            var obj = new JObject
            {
                ["Id"] = 1000,
                ["Alias"] = "Discount",
                ["Title"] = "Скидка"

            };
            var parameter = calculator.FindByKey(optionRoot, direction.GetKey()).First();
            ((JArray)parameter.SelectToken("Modifiers")).Add(obj);
            parameter["NumValue"] = 0.5;

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(42.5));
            Assert.That((decimal)result[0]["OldNumValue"], Is.EqualTo(85));
            Assert.That((bool)result[0]["Changed"], Is.True);
        }


        [Test]
        public void ProcessPackages_HasPackage_Applied()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple2_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(0));
            Assert.That((decimal)result[1]["NumValue"], Is.EqualTo(85));
            Assert.That((string)result[0]["Title"], Is.EqualTo("Новый заголовок (в пределах пакета)"));
            Assert.That((string)result[1]["Title"], Is.EqualTo("Новый заголовок (сверх пакета)"));
            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That((bool)result[1]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(2));
        }

        [Test]
        public void ProcessTarifficationSteps_HasTarifficationSteps_Applied()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple3_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            Assert.That(result.Length, Is.EqualTo(3));
            Assert.That((decimal)result[0]["NumValue"], Is.EqualTo(85));
            Assert.That((decimal)result[1]["NumValue"], Is.EqualTo(45));
            Assert.That((decimal)result[2]["NumValue"], Is.EqualTo(85));

            Assert.That((string)result[0]["Title"], Is.EqualTo("Новый заголовок (первый шаг)"));
            Assert.That((string)result[1]["Title"], Is.EqualTo("Новый заголовок (второй шаг)"));
            Assert.That((string)result[2]["Title"], Is.EqualTo("Новый заголовок (третий шаг)"));

            Assert.That((bool)result[0]["Changed"], Is.True);
            Assert.That((bool)result[1]["Changed"], Is.True);
            Assert.That((bool)result[2]["Changed"], Is.True);
            Assert.That(tariff.SelectTokens("Parameters.[?(@.Changed)]").Count(), Is.EqualTo(3));

        }

        [Test]
        public void FilterByCountryCode_ExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, new [] { option }, "UA");


            var cnt = tariff.SelectTokens(feeQuery).Count();

            var specialCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'UA')]").Count();
            var generalCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'WorldExceptRussia')]").Count();

            Assert.That(cnt, Is.LessThan(cntOption));
            Assert.That(cnt, Is.EqualTo(2));
            Assert.That(specialCnt, Is.EqualTo(2));
            Assert.That(generalCnt, Is.EqualTo(1));

        }

        [Test]
        public void FilterByCountryCode_ExistingFilterWithMissingWorldExceptRussia_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();

            var dir = new TariffDirection("SubscriptionFee", "WorldExceptRussia", null, null);
            var toRemove = calculator.FindByKey(option.SelectToken("Parameters"), dir.GetKey()).ToArray();
            foreach (JToken jToken in toRemove)
            {
                jToken.Remove();
            }
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();

            
            calculator.Calculate(tariff, new[] { option }, "UA");


            var cnt = tariff.SelectTokens(feeQuery).Count();

            var specialCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'UA')]").Count();
            var generalCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'WorldExceptRussia')]").Count();

            Assert.That(cnt, Is.LessThan(cntOption));
            Assert.That(cnt, Is.EqualTo(2));
            Assert.That(specialCnt, Is.EqualTo(2));
            Assert.That(generalCnt, Is.EqualTo(1));

        }

        [Test]
        public void FilterByCountryCode_PartialExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, new[] { option }, "HU");


            var cnt = tariff.SelectTokens(feeQuery).Count();
            var specials = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'HU')]").ToArray();
            var specialCnt = specials.Count();
            var generalCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'WorldExceptRussia')]").Count();


            Assert.That(cnt, Is.LessThan(cntOption));
            Assert.That(cnt, Is.EqualTo(2));
            Assert.That(specialCnt, Is.EqualTo(1));
            Assert.That(generalCnt, Is.EqualTo(2));
            Assert.That((string) specials.First()["Value"], Is.EqualTo("безлимитный"));
        }

        [Test]
        public void FilterByCountryCode_NonExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, new[] { option }, "LT");


            var cnt = tariff.SelectTokens(feeQuery).Count();
            var specials = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'LT')]").ToArray();
            var specialCnt = specials.Count();
            var generalCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true && @.Zone.Alias == 'WorldExceptRussia')]").Count();


            Assert.That(cnt, Is.LessThan(cntOption));
            Assert.That(cnt, Is.EqualTo(2));
            Assert.That(specialCnt, Is.EqualTo(0));
            Assert.That(generalCnt, Is.EqualTo(3));
        }


        [Test]
        public void FindByKey_DirectionWithMissedElements_Found()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var keys = root.SelectTokens("[?(@.BaseParameter)]").Select(n => n.ExtractDirection().GetKey());

            var result = new InternationalRoamingCalculator().FindByKey(root, direction.GetKey()).ToArray();

            Assert.That(result.Length, Is.EqualTo(1));
        }


    }
}
