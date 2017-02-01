using System;
using System.Linq;
using FluentAssertions;
using QA.Core.DPC.Loader.Tests.Extensions;
using QA.Core.Models.Entities;
using QA.Core.Models.Processors;
using QA.Core.Models.UI;
using Xunit;

namespace QA.Core.DPC.Loader.Tests
{
    public partial class ArticleEvalTests
    {
        private const string TestSampleDataFile = @"TestData\ReferenceDto.xaml";
        private const string TestGeProductFile = @"TestData\ge_product_click.xaml";
        private const string SkSimpleArticleFile = @"TestData\sk_simple.xaml";
        private const string SkSimpleArticleClassifierArticle = @"TestData\sk_simple_classifier.xaml";

        [MemberData(nameof(GetDPathExpressionData))]
        [Theory, Trait("DPathProcessor", "RegexParser")]
        public void DPathProcessor_WithDifferentPathExpressions_ShouldParseCorrectly(string expression, DPathArticleData[] expectedResult)
        {
            // Fixture setup
            // Exercise system
            var actualResult = DPathProcessor.VerifyAndParseExpression(expression).ToArray();

            // Verify outcome
            actualResult.ShouldBeEquivalentTo(expectedResult);
        }

        [XamlData(TestSampleDataFile, " PDF ")]
        [XamlData(TestSampleDataFile, "ProductWebEntities")]
        [XamlData(TestSampleDataFile, "Type/ ")]
        [XamlData(TestSampleDataFile, " Type / ProductFilters ")]
        [XamlData(TestSampleDataFile, " /Regions")]
        [XamlData(TestSampleDataFile, "Regions /  Title")]
        [XamlData(TestSampleDataFile, "MarketingProduct")]
        [XamlData(TestSampleDataFile, "MarketingProduct/Family")]
        [XamlData(TestSampleDataFile, "MarketingProduct/Categories")]
        [XamlData(TestSampleDataFile, "MarketingProduct/ProductType")]
        [XamlData(TestSampleDataFile, "MarketingProduct/UpSaleItems")]
        [XamlData(TestSampleDataFile, "[Type='305']")]
        [XamlData(TestSampleDataFile, "[Type='305'][Type='305']")]
        [XamlData(TestSampleDataFile, "[!Type='306'][!Type='306']")]
        [XamlData(TestSampleDataFile, "[SortOrder='300'][!SortOrder='200']")]
        [XamlData(TestSampleDataFile, "[!SortOrder='200'][SortOrder='300']")]
        [XamlData(TestSampleDataFile, "[SortOrder='300'][| SortOrder='400']")]
        [XamlData(TestSampleDataFile, "[SortOrder='400'][| SortOrder='300']")]
        [XamlData(TestSampleDataFile, "MarketingProduct[ProductType/ProductFilters/SortOrder='1']")]
        [XamlData(TestSampleDataFile, "MarketingProduct/ProductType/ProductFilters[SortOrder='1']")]
        [XamlData(TestSampleDataFile, "MarketingProduct/ProductType/ProductFilters[SortOrder='1']/Title")]
        [XamlData(TestSampleDataFile, "MarketingProduct[ProductType='289']")]
        [XamlData(TestSampleDataFile, "MarketingProduct[ProductType='289']/ProductType/ProductFilters[SortOrder='1']")]
        [XamlData(TestSampleDataFile, "MarketingProduct[ProductType='289']/ProductType/ProductFilters[SortOrder='1']/Title")]
        [XamlData(SkSimpleArticleFile, "[Type='ShopProduct']")]
        [XamlData(SkSimpleArticleClassifierArticle, "[Type='305']")]
        [XamlData(SkSimpleArticleClassifierArticle, "[Type='MobileTariff']")]
        [XamlData(TestSampleDataFile, "[Atype = '']")]

        [Theory, Trait("DPathProcessor", "SingleResult")]
        public void DPathProcessor_WhenProductFounded_ShouldReturnArrayWithSingleElement(string expression, Article product)
        {
            // Fixture setup
            const int expectedResult = 1;

            // Exercise system
            var actualResult = DPathProcessor.Process(expression, product);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult.Length);
        }

        [XamlData(TestGeProductFile, "Parameters[Modifiers/Alias='ExcludeFromPdf']", 0)]
        [XamlData(TestGeProductFile, "Parameters[!Modifiers/Alias='ExcludeFromPdf']", 8)]
        [XamlData(TestGeProductFile, "Parameters[!test_filter_expression='test_value']", 8)]
        [XamlData(TestGeProductFile, "test_field1[!test_filter_expression='test_value']", 0)]
        [XamlData(TestGeProductFile, "Parameters[BaseParameter/Alias = 'USSD'][| BaseParameter/Alias = 'BeeNumber']", 3)]
        [XamlData(TestGeProductFile, "Parameters[BaseParameter/Alias = 'USSD'][BaseParameter/Title = 'USSD для перехода'][| BaseParameter/Alias = 'BeeNumber'][ BaseParameter/ Title = 'Биномер для перехода']", 3)]
        [XamlData(TestGeProductFile, "Parameters[BaseParameter/Alias = '']", 5)]

        [Theory, Trait("DPathProcessor", "MultipleResults")]
        public void DPathProcessor_WhenProductFounded_ShouldReturnArrayWithMultipleElements(string expression, Article product, int expectedResult)
        {
            // Fixture setup
            // Exercise system
            var actualResult = DPathProcessor.Process(expression, product);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult.Length);
        }

        [XamlData(TestSampleDataFile, "test_field1/test_field2/test_field3")]
        [XamlData(TestSampleDataFile, "MarketingProduct/Regions")]
        [XamlData(TestSampleDataFile, "MarketingProduct[test_filter_expression='test_value']")]
        [XamlData(TestSampleDataFile, "MarketingProduct[ProductType='test_value']")]
        [Theory, Trait("DPathProcessor", "NotExistingProducts")]
        [XamlData(SkSimpleArticleFile, "[Type='305']")]
        [XamlData(SkSimpleArticleClassifierArticle, "[Type='ShopProduct']")]
        public void DPathProcessor_WhenProductNotFounded_ShouldReturnEmptyArray(string expression, Article product)
        {
            // Fixture setup
            var expectedResult = Enumerable.Empty<ModelObjectWithParent>().ToArray();

            // Exercise system
            var actualResult = DPathProcessor.Process(expression, product);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [XamlData(TestSampleDataFile, " !@#$_just_any_%^&_test_text_*()+ ")]
        [XamlData(TestSampleDataFile, "%MarketingProduct[ProductType='test_value']")]
        [XamlData(TestSampleDataFile, " P*DF ")]
        [Theory, Trait("DPathProcessor", "NotExistingProducts")]
        public void DPathProcessor_GivenWrongExpression_ShouldThrowException(string expression, Article product)
        {
            // Fixture setup
            // Exercise system
            Action actionResult = () => DPathProcessor.Process(expression, product);

            // Verify outcome
            Assert.Throws<Exception>(actionResult);
        }
    }
}
