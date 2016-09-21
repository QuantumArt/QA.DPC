using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using QA.Core.Models.Entities;

namespace QA.Core.Models.Processors
{
    public class OldDPathProcessor
    {
        public static readonly Regex VirtualFieldPathRegex = new Regex(@"\A(?:(?'field'\w*)(?:\[(?'inversionOperator'!)?(?'filterPath'(?:\w+/?)+)='(?'filterValue'.*?)'\])*/?)+\z", RegexOptions.Compiled);

        public const string FILTER_VALUE_REGEX_GROUP_NAME = "filterValue";

        public const string FIELD_REGEX_GROUP_NAME = "field";

        private class FieldFilterInfo
        {
            public string Path;

            public string Value;

            public bool Inverted;
        }

        public static ModelObjectWithParent[] EvaluatePath(string path, Article context)
        {
            var pathMatch = VirtualFieldPathRegex.Match(path);

            if (!pathMatch.Success)
                throw new ArgumentException("Некорректный формат пути: " + path);

            ModelObjectWithParent[] resultModelObjects = { new ModelObjectWithParent { ModelObject = context } };

            var filterPathsCaptures = pathMatch.Groups["filterPath"].Captures.Cast<Capture>().ToArray();

            var filterValsCaptures = pathMatch.Groups[FILTER_VALUE_REGEX_GROUP_NAME].Captures.Cast<Capture>();

            var fieldNamesCaptures = pathMatch.Groups[FIELD_REGEX_GROUP_NAME].Captures;

            var inversionOperatorsCaptures = pathMatch.Groups["inversionOperator"].Captures.Cast<Capture>().ToArray();

            for (int i = 0; i < fieldNamesCaptures.Count; i++)
            {
                Capture fieldCapture = fieldNamesCaptures[i];

                string fieldName = fieldCapture.Value;

                if (!string.IsNullOrEmpty(fieldName))
                {
                    if (resultModelObjects.All(x => x.ModelObject is IGetArticleField))
                        resultModelObjects = resultModelObjects
                        .Select(x => new ModelObjectWithParent { Parent = x.ModelObject, ModelObject = ((IGetArticleField)x.ModelObject).GetField(fieldName) })
                        .Where(x => x.ModelObject != null)
                        .ToArray();
                    else if (resultModelObjects.All(x => x.ModelObject is IEnumerable<Article>))
                        resultModelObjects = resultModelObjects
                            .SelectMany(x => ((IEnumerable<Article>)x.ModelObject)
                                            .Select(y => new ModelObjectWithParent { ModelObject = y.Fields[fieldName], Parent = y }))
                            .ToArray();
                }

                if (!resultModelObjects.Any())
                    return new ModelObjectWithParent[0];

                var filterPathCurrCaptures = filterPathsCaptures
                  .Where(x => x.Index > fieldCapture.Index && (i == fieldNamesCaptures.Count - 1 || x.Index < fieldNamesCaptures[i + 1].Index))
                  .ToArray();

                var inversionOperatorCurrCaptures = inversionOperatorsCaptures
                  .Where(x => x.Index > fieldCapture.Index && (i == fieldNamesCaptures.Count - 1 || x.Index < fieldNamesCaptures[i + 1].Index))
                  .ToArray();

                if (filterPathCurrCaptures.Any())
                {
                    var filterInfos = filterPathCurrCaptures
                      .Select((x, index) =>
                      {
                          var valCapture = filterValsCaptures.SingleOrDefault(y => y.Index > x.Index && (index == filterPathCurrCaptures.Length - 1 || y.Index < filterPathCurrCaptures[index + 1].Index));

                          bool inverted =
                  inversionOperatorCurrCaptures.Any(
                    y =>
                      (index == 0 || y.Index > filterPathCurrCaptures[index - 1].Index) &&
                      (index == filterPathCurrCaptures.Length - 1 || y.Index < filterPathCurrCaptures[index + 1].Index));

                          return new FieldFilterInfo { Path = x.Value, Value = valCapture == null ? string.Empty : valCapture.Value, Inverted = inverted };
                      })
                      .ToArray();

                    resultModelObjects = resultModelObjects
                      .SelectMany(x => GetChildArticlesOrSelf(x.ModelObject)
                                      .Where(y =>
                          filterInfos
                            .All(filterInfo =>
                            {
                                var filterPath = EvaluatePath(filterInfo.Path, y);
                                bool filterConditionMached = filterPath.Any(
                          foundFilterObject => //если пришло много полей то подразумеваем что = значит что хотя бы одно из них равно
                        {
                            if (foundFilterObject.ModelObject is Article)
                                throw new Exception("фильтры внутри фильтров не поддерживаются");

                            ArticleField fieldToFilter = (ArticleField)foundFilterObject.ModelObject;

                            if (!(fieldToFilter is IGetFieldStringValue))
                                throw new Exception("Фильтровать можно только по значением полей типа IGetFieldStringValue");

                            return ((IGetFieldStringValue)fieldToFilter).Value == filterInfo.Value;
                        }
                        );

                                return filterConditionMached && !filterInfo.Inverted || !filterConditionMached && filterInfo.Inverted;
                            }
                            ))
                        .Select(y => new ModelObjectWithParent { ModelObject = y, Parent = x.ModelObject })
                      )
                      .ToArray();

                    if (!resultModelObjects.Any())
                        break;
                }
            }

            return resultModelObjects;
        }

        private static IEnumerable<Article> GetChildArticlesOrSelf(IModelObject modelObject)
        {
            if (modelObject is IEnumerable<Article>)
                return (IEnumerable<Article>)modelObject;
            else if (modelObject is IGetArticle)
            {
                var article = ((IGetArticle)modelObject).GetItem(null);

                return article == null ? new Article[] { } : new[] { article };
            }
            else if (modelObject is Article)
                return new[] { (Article)modelObject };
            else
                throw new Exception("modelObject должен быть Article, IEnumerable<Article> или IGetArticle");

            throw new ArgumentException("context must be Article");
        }

        public class ModelObjectWithParent
        {
            public IModelObject ModelObject { get; set; }

            public IModelObject Parent { get; set; }
        }
    }
}
