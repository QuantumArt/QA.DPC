using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QA.Core.Models.Entities;
using QA.Core.Models.UI;

namespace QA.Core.Models.Processors
{
    public class DPathProcessor
    {
        private const string FieldRegexGroupName = "field";
        private const string FilterExpressionRegexGroupName = "filterExpression";
        private const string FilterLogicalRegexGroupName = "logical";
        private const string FilterInverseRegexGroupName = "inverse";
        private const string FilterPathRegexGroupName = "filterPath";
        private const string FilterValueRegexGroupName = "filterValue";

        private static readonly string ExpressionRegexString = $@"
        (?:\s*
            (?<{FieldRegexGroupName}>\w*)
            (?<{FilterExpressionRegexGroupName}>
            \s*\[\s*
                (?<{FilterLogicalRegexGroupName}>[\|\&]?)\s*
                (?<{FilterInverseRegexGroupName}>!?)\s*
                (?<{FilterPathRegexGroupName}>(?:\s*\w+\s*\/?)+)\s*
                =\s*
                '(?<{FilterValueRegexGroupName}>[^']*)'
            \s*\]\s*
            )*\s*\/\s*
        )";

        private static readonly Regex ExpressionRegex = new Regex(ExpressionRegexString, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        private static readonly Regex ExpressionRegexToValidate = new Regex($"^{ExpressionRegexString}+$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Ищет в context поле или статью по path.
        /// EBNF Syntax: { [ field_name ] [ "[" [|&] [!] { field_name } [ /...n ] "=" 'field_value' "]" [ ...n ] ] } [ /...n ]
        /// [ ] cлужат для фильтрациии
        /// ! - признак логической инверсии условия фильтрации
        /// | (OR) и & (AND)- булевое объединение текущего фильтра с предыдущим
        /// </summary>
        /// <returns><see cref="Article"/>, - если в конце фильтр, иначе - <see cref="ArticleField"/></returns>
        public static ModelObjectWithParent[] Process(string expression, Article articleNode)
        {
            var articlesData = VerifyAndParseExpression(expression);
            var result = new[] { new ModelObjectWithParent { ModelObject = articleNode } };

            foreach (var articleData in articlesData)
            {
                if (!string.IsNullOrWhiteSpace(articleData.FieldName))
                {
                    result = GetResultForSingleOrMultiArticleByFieldName(result, articleData.FieldName).ToArray();
                }

                if (articleData.FiltersData.Any())
                {
                    result = FilterResult(result, articleData.FiltersData).ToArray();
                }
            }

            return result;
        }

        public static IEnumerable<DPathArticleData> VerifyAndParseExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var normalizedExpression = NormalizeExpression(expression);

            if (!IsExpressionValid(normalizedExpression))
            {
                throw new Exception($"Wrong expression data: {normalizedExpression}");
            }

            var matches = ExpressionRegex.Matches(normalizedExpression);
            foreach (Match match in matches)
            {
                var filterExpressions = match.Groups[FilterPathRegexGroupName].Captures.Cast<Capture>().Select(fe => fe.Value).ToArray();
                var filterValues = match.Groups[FilterValueRegexGroupName].Captures.Cast<Capture>().Select(fv => fv.Value).ToArray();
                var filterLogical = match.Groups[FilterLogicalRegexGroupName].Captures.Cast<Capture>().Select(fe => fe.Value).ToArray();
                var filterInverse = match.Groups[FilterInverseRegexGroupName].Captures.Cast<Capture>().Select(fe => fe.Value).ToArray();

                if (filterExpressions.Length != filterValues.Length || filterExpressions.Length != filterLogical.Length || filterExpressions.Length != filterInverse.Length)
                {
                    throw new Exception("There was an error while parsing expression");
                }

                var articleData = new DPathArticleData
                {
                    FieldName = match.Groups[FieldRegexGroupName].Value,
                    FiltersData = filterExpressions.Zip(filterValues, (fev, fvv) => new DPathFilterData
                    {
                        Expression = fev.Trim(),
                        Value = fvv.Trim()
                    }).Zip(filterInverse, (entry, fi) => new DPathFilterData
                    {
                        Expression = entry.Expression,
                        Value = entry.Value,
                        IsInversed = fi == "!"
                    }).Zip(filterLogical, (entry, fl) => new DPathFilterData
                    {
                        Expression = entry.Expression,
                        Value = entry.Value,
                        IsInversed = entry.IsInversed,
                        IsDisjuncted = fl == "|"
                    }).ToList()
                };

                yield return articleData;
            }
        }

        public static string NormalizeExpression(string expression)
        {
            var result = expression.Trim();
            if (expression.First() == '/')
            {
                result = new string(result.Skip(1).ToArray());
            }

            if (expression.Last() != '/')
            {
                result += "/";
            }

            return result;
        }

        public static bool IsExpressionValid(string expression)
        {
            return ExpressionRegexToValidate.IsMatch(expression);
        }

        private static IEnumerable<ModelObjectWithParent> FilterResult(IEnumerable<ModelObjectWithParent> resultData, IList<DPathFilterData> articleFiltersData)
        {
            foreach (var resultEntry in resultData)
            {
                var childArticlesOrSelf = GetChildArticlesOrSelf(resultEntry.ModelObject);
                foreach (var article in childArticlesOrSelf)
                {
                    var disjunctionResults = new List<bool>();
                    var conjunctionResult = true;

                    foreach (var t in articleFiltersData)
                    {
                        var partialResult = IsArticlePassFilter(article, t);
                        if (!t.IsDisjuncted)
                        {
                            conjunctionResult &= partialResult;
                        }
                        else
                        {
                            disjunctionResults.Add(conjunctionResult);
                            conjunctionResult = partialResult;
                        }
                    }

                    bool result;
                    if (disjunctionResults.Any())
                    {
                        disjunctionResults.Add(conjunctionResult);
                        result = disjunctionResults.Any(n => n);
                    }
                    else
                    {
                        result = conjunctionResult;
                    }

                    if (result)
                    {
                        yield return new ModelObjectWithParent
                        {
                            ModelObject = article,
                            Parent = resultEntry.ModelObject
                        };
                    }
                }
            }
        }

        private static bool IsArticlePassFilter(Article article, DPathFilterData filterData)
        {
            var articlesInFilter = Process(filterData.Expression, article);
            if (!articlesInFilter.Any() && (filterData.IsInversed || string.IsNullOrEmpty(filterData.Value)))
            {
                return true;
            }

            return articlesInFilter
                .Select(articleInFilter => (IGetFieldStringValue)articleInFilter.ModelObject)
                .Select(articleToCheck => IsEqualToFilter(articleToCheck, filterData))
                .FirstOrDefault();
        }

        private static bool IsEqualToFilter(IGetFieldStringValue articleToCheck, DPathFilterData filterData)
        {
            if (articleToCheck == null)
            {
                throw new Exception("May be filter expression was wrong (should implement IGetFieldStringValue interface)");
            }

            var result = articleToCheck.Value == filterData.Value;

            var ext = articleToCheck as ExtensionArticleField;
            if (ext != null)
            {
                result |= ext.Item?.ContentName == filterData.Value;
            }

            return (filterData.IsInversed) ? !result : result;
        }

        private static IEnumerable<ModelObjectWithParent> GetResultForSingleOrMultiArticleByFieldName(IList<ModelObjectWithParent> resultData, string fieldName)
        {
            if (resultData.All(r => r.ModelObject is IGetArticleField))
            {
                return resultData.Select(mop => new ModelObjectWithParent
                {
                    Parent = mop.ModelObject,
                    ModelObject = ((IGetArticleField)mop.ModelObject).GetField(fieldName)
                }).Where(mop => mop.ModelObject != null);
            }

            if (resultData.All(r => r.ModelObject is IEnumerable<Article>))
            {
                return resultData.SelectMany(mop => ((IEnumerable<Article>)mop.ModelObject).Select(article => new ModelObjectWithParent
                {
                    Parent = article,
                    ModelObject = article.Fields[fieldName]
                }));
            }

            throw new NotImplementedException("Unexpected data type of ModelObjects. Should be IGetArticleField or IEnumerable<Article>");
        }

        private static IEnumerable<Article> GetChildArticlesOrSelf(IModelObject modelObject)
        {
            var multipleArticles = modelObject as IEnumerable<Article>;
            if (multipleArticles != null)
            {
                return multipleArticles;
            }

            var singleArticle = modelObject as IGetArticle;
            if (singleArticle != null)
            {
                var article = singleArticle.GetItem(null);
                return article == null ? new Article[] { } : new[] { article };
            }

            var articleField = modelObject as Article;
            if (articleField != null)
            {
                return new[] { articleField };
            }

            throw new ArgumentException("Unexpected data type of ModelObject, should be IGetArticles, IGetArticle или IGetArticleField", nameof(modelObject));
        }
    }
}
