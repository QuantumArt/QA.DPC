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
        private const string TestDataFile = @"TestData\ReferenceDto.xaml";

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

        [XamlData(TestDataFile, " PDF ")]
        [XamlData(TestDataFile, "ProductWebEntities")]
        [XamlData(TestDataFile, "Type/ ")]
        [XamlData(TestDataFile, " Type / ProductFilters ")]
        [XamlData(TestDataFile, " /Regions")]
        [XamlData(TestDataFile, "Regions /  Title")]
        [XamlData(TestDataFile, "MarketingProduct")]
        [XamlData(TestDataFile, "MarketingProduct/Family")]
        [XamlData(TestDataFile, "MarketingProduct/Categories")]
        [XamlData(TestDataFile, "MarketingProduct/ProductType")]
        [XamlData(TestDataFile, "MarketingProduct/UpSaleItems")]
        [XamlData(TestDataFile, "[Type='305']")]
        [XamlData(TestDataFile, "[Type='305'][Type='305']")]
        [XamlData(TestDataFile, "[!Type='306'][!Type='306']")]
        [XamlData(TestDataFile, "[SortOrder='300'][!SortOrder='200']")]
        [XamlData(TestDataFile, "[!SortOrder='200'][SortOrder='300']")]
        [XamlData(TestDataFile, "MarketingProduct[ProductType/ProductFilters/SortOrder='1']")]
        [XamlData(TestDataFile, "MarketingProduct/ProductType/ProductFilters[SortOrder='1']")]
        [XamlData(TestDataFile, "MarketingProduct/ProductType/ProductFilters[SortOrder='1']/Title")]
        [XamlData(TestDataFile, "MarketingProduct[ProductType='289']")]
        [XamlData(TestDataFile, "MarketingProduct[ProductType='289']/ProductType/ProductFilters[SortOrder='1']")]
        [XamlData(TestDataFile, "MarketingProduct[ProductType='289']/ProductType/ProductFilters[SortOrder='1']/Title")]
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

        [XamlData(TestDataFile, "test_field1/test_field2/test_field3")]
        [XamlData(TestDataFile, "MarketingProduct/Regions")]
        [XamlData(TestDataFile, "MarketingProduct[test_filter_expression='test_value']")]
        [XamlData(TestDataFile, "MarketingProduct[ProductType='test_value']")]
        [Theory, Trait("DPathProcessor", "NotExistingProducts")]
        public void DPathProcessor_WhenProductNotFounded_ShouldReturnEmptyArray(string expression, Article product)
        {
            // Fixture setup
            var expectedResult = Enumerable.Empty<ModelObjectWithParent>().ToArray();

            // Exercise system
            var actualResult = DPathProcessor.Process(expression, product);

            // Verify outcome
            Assert.Equal(expectedResult, actualResult);
        }

        [XamlData(TestDataFile, " !@#$_just_any_%^&_test_text_*()+ ")]
        [XamlData(TestDataFile, "%MarketingProduct[ProductType='test_value']")]
        [XamlData(TestDataFile, " P*DF ")]
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
