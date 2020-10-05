using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace QA.ProductCatalog.ImpactService.Tests
{
    public class BaseImpactCalculatorFixture
    {
        private JObject GetJsonFromFile(string file)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"TestData\\{file}").Replace('\\', Path.DirectorySeparatorChar);
            return JObject.Parse(File.ReadAllText(path));
        }

        [Fact]
        public void ChangeParameters_MixedOrder_Reordered()
        {
            var tariff = GetJsonFromFile("simple_tariff_order.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, new JObject[] {}, null);

            var orders = new[] {34, 33, 32, 1000, 11, 12, 2000, 21, 22, 1};

            var resultOrders = tariff.SelectTokens("Parameters.[?(@.Id)].Id").Select(n => (int) n).ToArray();

            resultOrders.Should().BeEquivalentTo(orders);

        }

        [Fact]
        public void ChangeParameters_NumValueSmaller_Changed()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(45);
            ((decimal)result[0]["OldNumValue"]).Should().Be(85);
            ((string)result[0]["Title"]).Should().NotBe("Новый заголовок");
            ((bool)result[0]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(1);
        }


        [Fact]
        public void ChangeParameters_NumValueSmallerWithLinkMerging_Changed()
        {
            var tariff = GetJsonFromFile("simple2_tariff.json");
            var option = GetJsonFromFile("simple1_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(65);
            ((string)result[0]["Title"]).Should().NotBe("Новый заголовок");
            ((bool)result[0]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(1);
        }

        [Fact]
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
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            result[0]["Changed"].Should().BeNull();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(0);
        }

        [Fact]
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
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            result[0]["Changed"].Should().BeNull();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(0);
        }


        [Fact]
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
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            result[0]["Changed"].Should().BeNull();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(0);
        }

        [Fact]
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
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(95);
            ((bool)result[0]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(1);
        }

        [Fact]
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
            result.Length.Should().Be(0);
            tariff.SelectTokens("Parameters.[?(@.Id)]").Count().Should().BeGreaterThan(0);
        }

        [Fact]
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
            result.Length.Should().Be(0);
            cntAfter.Should().Be(cntBefore - 1);
        }

        [Fact]
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
            calculator.Reorder(tariff, null);            

            var cntAfter = tariff.SelectTokens("Parameters.[?(@.Id)]").Count();
            var root = tariff.SelectToken("Parameters");
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            result[0].Previous["Changed"].Should().NotBeNull();

            cntAfter.Should().Be(cntBefore + 1);
        }

        [Fact]
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
            result.Length.Should().Be(1);
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            cntAfter.Should().Be(cntBefore + 1);
        }




        [Fact]
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

            result.Length.Should().Be(1);
            cntAfter.Should().Be(cntBefore + 1);
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            ((decimal)result2["NumValue"]).Should().Be(45);
            result[0]["Changed"].Should().BeNull();
            ((bool)result2["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(1);
        }

        [Fact]
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
            result.Length.Should().Be(1);
            cntAfter.Should().Be(cntBefore);
            ((decimal)result[0]["NumValue"]).Should().Be(45);
            ((string)result[0]["Title"]).Should().NotBe("Новый заголовок");
            ((bool)result[0]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(1);
        }


        [Fact]
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
            result.Length.Should().Be(1);
            cntAfter.Should().Be(cntBefore);
            ((decimal)result[0]["NumValue"]).Should().Be(130);
            ((string)result[0]["Title"]).Should().NotBe("Новый заголовок");
            ((bool)result[0]["Changed"]).Should().BeTrue();
        }

        [Fact]
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
            result.Length.Should().Be(1);
            ((double)result[0]["NumValue"]).Should().Be(42.5);
            ((decimal)result[0]["OldNumValue"]).Should().Be(85);
            ((bool)result[0]["Changed"]).Should().BeTrue();
        }

        [Fact]
        public void ProcessPackages_HasGroupPackage_Applied()
        {
            var tariff = GetJsonFromFile("package_group_tariff.json");
            var option = GetJsonFromFile("package_group_option.json");
            var calculator = new TariffOptionCalculator();

            calculator.Calculate(tariff, option);
            calculator.Reorder(tariff, null);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            var direction2 = new TariffDirection("ParameterGroup", null, null, new []{ "WithinPackage" });
            var result2 = calculator.FindByKey(root, direction2.GetKey()).ToArray();
            var direction3 = new TariffDirection("ParameterGroup", null, null, new[] { "OverPackage" });
            var result3 = calculator.FindByKey(root, direction3.GetKey()).ToArray();


            result.Length.Should().Be(2);
            result2.Length.Should().Be(1);
            result3.Length.Should().Be(1);



            ((decimal)result[0]["NumValue"]).Should().Be(0);
            ((decimal)result[1]["NumValue"]).Should().Be(85);
            ((string)result[0]["Title"]).Should().Be("Новый заголовок (в пределах пакета)");
            ((string)result[1]["Title"]).Should().Be("Новый заголовок (сверх пакета)");
            ((bool)result[0]["Changed"]).Should().BeTrue();
            ((bool)result[1]["Changed"]).Should().BeTrue();

            result[0].Previous.Should().BeEquivalentTo(result2[0]);
            result[1].Previous.Should().BeEquivalentTo(result3[0]);

            ((string)result2[0]["Title"]).Should().Be("В пределах пакета 500 минут");
            ((int)result2[0]["Id"]).Should().Be(5000);
            result2[0].Previous.Should().NotBeNull();

            ((string)result3[0]["Title"]).Should().Be("Сверх пакета 500 минут");
            ((int)result3[0]["Id"]).Should().Be(6000);
            result3[0].Previous.Should().NotBeNull();

            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(6);
        }

        [Fact]
        public void ProcessParents_Reparent_Applied()
        {
            var tariff = GetJsonFromFile("simple_reparent_tariff.json");
            var option = GetJsonFromFile("simple_reparent_option.json");
            var calculator = new TariffOptionCalculator();

            calculator.Calculate(tariff, option);
            calculator.Reorder(tariff, null);            

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, null, null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            var direction2 = new TariffDirection("ParameterGroup", null, null, new[] { "First" });
            var result2 = calculator.FindByKey(root, direction2.GetKey()).ToArray();
            var direction3 = new TariffDirection("ParameterGroup", null, null, new[] { "Second" });
            var result3 = calculator.FindByKey(root, direction3.GetKey()).ToArray();
            var direction4 = new TariffDirection("IncomingCalls", null, null, null);
            var result4 = calculator.FindByKey(root, direction4.GetKey()).ToArray();
            var direction5 = new TariffDirection("MinutesPackage", null, null, null);
            var result5 = calculator.FindByKey(root, direction5.GetKey()).ToArray();


            result4.Length.Should().Be(1);
            result5.Length.Should().Be(1);

            ((int)result[0]["Parent"]["Id"]).Should().Be((int) result3[0]["Id"]);
            ((string)result2[0]["Title"]).Should().Be("Подгруппа 1");
            ((string)result3[0]["Title"]).Should().Be("Подгруппа 2 (новая)");
            ((int)result4[0]["Parent"]["Id"]).Should().Be((int)result2[0]["Id"]);
            ((int)result5[0]["Parent"]["Id"]).Should().Be((int)result2[0]["Id"]);
        }

        [Fact]
        public void ProcessPackage_ForcedInfluence_Applied()
        {
            var tariff = GetJsonFromFile("simple_package_force_tariff.json");
            var option = GetJsonFromFile("simple_package_force_option.json");
            var calculator = new TariffOptionCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, null, new[] { "WithinPackage" });
            var result = calculator.FindByKey(root, direction.GetKey()).ToArray();
            var direction2 = new TariffDirection("OutgoingCalls", null, null, new[] { "OverPackage" });
            var result2 = calculator.FindByKey(root, direction2.GetKey()).ToArray();

            ((string)result[0]["Title"]).Should().Be("Звонки по всей России (в пределах пакета)");
            ((string)result2[0]["Title"]).Should().Be("Звонки (сверх пакета)");

            ((bool)result[0]["Changed"]).Should().BeTrue();
            result2[0]["Changed"].Should().BeNull();
            result[0]["NumValue"].Should().NotBeNull();
            result2[0]["NumValue"].Should().NotBeNull();
        }

        [Fact]
        public void ProcessPackages_HasPackage_Applied()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("package_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            result.Length.Should().Be(2);
            ((decimal)result[0]["NumValue"]).Should().Be(0);
            ((decimal)result[1]["NumValue"]).Should().Be(85);
            ((string)result[0]["Title"]).Should().Be("Новый заголовок (в пределах пакета)");
            ((string)result[1]["Title"]).Should().Be("Новый заголовок (сверх пакета)");
            ((bool)result[0]["Changed"]).Should().BeTrue();
            ((bool)result[1]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(2);
        }

        [Fact]
        public void ProcessTarifficationSteps_HasTarifficationSteps_Applied()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple3_option.json");
            var calculator = new InternationalRoamingCalculator();

            calculator.Calculate(tariff, option);

            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var result = calculator.FindByKey(root, direction.GetKey(), true).ToArray();
            result.Length.Should().Be(3);
            ((decimal)result[0]["NumValue"]).Should().Be(85);
            ((decimal)result[1]["NumValue"]).Should().Be(45);
            ((decimal)result[2]["NumValue"]).Should().Be(85);

            ((string)result[0]["Title"]).Should().Be("Новый заголовок (первый шаг)");
            ((string)result[1]["Title"]).Should().Be("Новый заголовок (второй шаг)");
            ((string)result[2]["Title"]).Should().Be("Новый заголовок (третий шаг)");

            ((bool)result[0]["Changed"]).Should().BeTrue();
            ((bool)result[1]["Changed"]).Should().BeTrue();
            ((bool)result[2]["Changed"]).Should().BeTrue();
            tariff.SelectTokens("Parameters.[?(@.Changed)]").Count().Should().Be(3);

        }

        [Fact]
        public void FilterByCountryCode_ExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, null, new [] { option }, "UA", null);


            var cnt = tariff.SelectTokens(feeQuery).Count();

            var changedCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true)]").Count();

            cnt.Should().BeLessThan(cntOption);
            cnt.Should().Be(2);
            changedCnt.Should().Be(3);

        }

        [Fact]
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

            
            calculator.Calculate(tariff, null, new[] { option }, "UA", null);


            var cnt = tariff.SelectTokens(feeQuery).Count();

            var changedCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true)]").Count();

            cnt.Should().BeLessThan(cntOption);
            cnt.Should().Be(2);
            changedCnt.Should().Be(3);
        }

        [Fact]
        public void FilterByCountryCode_PartialExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, null, new[] { option }, "HU", null);


            var cnt = tariff.SelectTokens(feeQuery).Count();
            var changed = tariff.SelectTokens("Parameters.[?(@.Changed == true)]");
            var changedCnt = changed.Count();

            cnt.Should().BeLessThan(cntOption);
            cnt.Should().Be(2);
            changedCnt.Should().Be(3);
            var parameter = changed.Single(n =>
                n.SelectTokens("BaseParameterModifiers.[?(@.Alias)].Alias").Select(m => m.ToString()).ToArray()
                    .Contains("Unlimited"));
            ((string) parameter["Value"]).Should().Be("безлимитный");
        }

        [Fact]
        public void FilterByCountryCode_NonExistingFilter_Filtered()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var option = GetJsonFromFile("simple4_option.json");
            var calculator = new InternationalRoamingCalculator();
            string feeQuery = "Parameters.[?(@.BaseParameter.Alias == 'SubscriptionFee')]";
            var cntOption = option.SelectTokens(feeQuery).Count();
            calculator.Calculate(tariff, null, new[] { option }, "LT", null);


            var cnt = tariff.SelectTokens(feeQuery).Count();
            var changedCnt = tariff.SelectTokens("Parameters.[?(@.Changed == true)]").Count();


            cnt.Should().BeLessThan(cntOption);
            cnt.Should().Be(2);
            changedCnt.Should().Be(3);
        }


        [Fact]
        public void FindByKey_DirectionWithMissedElements_Found()
        {
            var tariff = GetJsonFromFile("simple1_tariff.json");
            var root = tariff.SelectToken("Parameters");
            var direction = new TariffDirection("OutgoingCalls", null, "Russia", null);
            var keys = root.SelectTokens("[?(@.BaseParameter)]").Select(n => n.ExtractDirection().GetKey());

            var result = new InternationalRoamingCalculator().FindByKey(root, direction.GetKey()).ToArray();

            result.Length.Should().Be(1);
        }


    }
}
