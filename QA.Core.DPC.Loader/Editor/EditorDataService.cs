using QA.Core.DPC.QP.Services;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader.Editor
{
    public class EditorDataService
    {
        private readonly DBConnector _dbConnector;
        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;

        public EditorDataService(
            IConnectionProvider connectionProvider, 
            ContentService contentService,
            FieldService fieldService)
        {
            _dbConnector = new DBConnector(connectionProvider.GetConnection());
            _contentService = contentService;
            _fieldService = fieldService;
        }
        
        private class ArticleContext
        {
            public IArticleFilter Filter;

            public Dictionary<int, ContentObject> ShapesByContentId;

            public bool ShouldIncludeArticle(Article article)
            {
                return article != null
                    && article.Visible
                    && !article.Archived
                    && Filter.Matches(article);
            }

            public bool ShouldIncludeField(ArticleField field)
            {
                switch (field)
                {
                    case VirtualArticleField _:
                    case VirtualMultiArticleField _:
                        return false;
                    default:
                        return true;
                }
            }

            public ContentObject MakeObjectForArticle(Article article)
            {
                if (!ShapesByContentId.TryGetValue(article.ContentId, out ContentObject contentShape))
                {
                    throw new InvalidOperationException(
                        $"Product does not contain definition for content {article.ContentId} from article {article.Id}");
                }

                return (ContentObject)contentShape.Clone();
            }
        }

        /// <exception cref="InvalidOperationException" />
        /// <exception cref="NotSupportedException" />
        public ContentObject ConvertArticle(
            Article article, IArticleFilter filter, Dictionary<int, ContentObject> shapesByContentId)
        {
            return ConvertArticle(article, new ArticleContext
            {
                Filter = filter,
                ShapesByContentId = shapesByContentId,
            });
        }

        /// <exception cref="InvalidOperationException" />
        /// <exception cref="NotSupportedException" />
        private ContentObject ConvertArticle(Article article, ArticleContext context)
        {
            if (!context.ShouldIncludeArticle(article))
            {
                return null;
            }
            if (!context.ShapesByContentId.TryGetValue(article.ContentId, out ContentObject contentShape))
            {
                throw new InvalidOperationException(
                    $"Product does not contain definition for content ({article.ContentId}) from article {article.ContentName}({article.Id})");
            }

            var dict = (ContentObject)contentShape.Clone();

            dict[EditorObjectShapeService.IdProp] = article.Id;

            foreach (ArticleField field in article.Fields.Values)
            {
                if (context.ShouldIncludeField(field))
                {
                    PopulateField(dict, field, context);
                }
            }

            return dict;
        }

        /// <exception cref="InvalidOperationException" />
        /// <exception cref="NotSupportedException" />
        private void PopulateField(ContentObject dict, ArticleField field, ArticleContext context)
        {
            if (field is PlainArticleField plainArticleField)
            {
                dict[field.FieldName] = ConvertPlainField(plainArticleField);
            }
            else if (field is SingleArticleField singleArticleField)
            {
                dict[field.FieldName] = ConvertArticle(singleArticleField.GetItem(context.Filter), context);
            }
            else if (field is MultiArticleField multiArticleField)
            {
                dict[field.FieldName] = multiArticleField
                    .GetArticles(context.Filter)
                    .Select(f => ConvertArticle(f, context))
                    .ToArray();
            }
            else if (field is ExtensionArticleField extensionArticleField)
            {
                PopulateExtensionField(dict, extensionArticleField, context);
            }
            else
            {
                throw new NotSupportedException($"Поле типа {field.GetType()} не поддерживается");
            }
        }

        /// <exception cref="InvalidOperationException" />
        private void PopulateExtensionField(
            ContentObject dict, ExtensionArticleField field, ArticleContext context)
        {
            if (field.Item == null)
            {
                return;
            }

            Article article = field.Item;

            if (dict.TryGetValue(field.FieldName, out object fieldValue)
                && fieldValue is ExtensionFieldObject extensionFieldObject)
            {
                extensionFieldObject.Value = article.ContentName;
                extensionFieldObject.Contents[article.ContentName] = ConvertArticle(article, context);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Product does not contain definition for extension field {field.FieldName}");
            }
        }

        private object ConvertPlainField(PlainArticleField plainArticleField)
        {
            if (plainArticleField.NativeValue == null)
            {
                return null;
            }

            switch (plainArticleField.PlainFieldType)
            {
                case PlainFieldType.File:
                {
                    if (String.IsNullOrWhiteSpace(plainArticleField.Value))
                    {
                        return null;
                    }

                    string path = Common.GetFileFromQpFieldPath(
                        _dbConnector, plainArticleField.FieldId.Value, plainArticleField.Value);

                    return new FileFieldObject
                    {
                        Name = plainArticleField.Value.Contains("/")
                            ? plainArticleField.Value.Substring(plainArticleField.Value.LastIndexOf("/") + 1)
                            : plainArticleField.Value,

                        AbsoluteUrl = String.Format("{0}/{1}",
                            _dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
                            plainArticleField.Value)
                    };
                }

                case PlainFieldType.Image:
                case PlainFieldType.DynamicImage:
                {
                    if (String.IsNullOrWhiteSpace(plainArticleField.Value))
                    {
                        return null;
                    }

                    return String.Format("{0}/{1}",
                        _dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
                        plainArticleField.Value);
                }

                case PlainFieldType.Boolean:
                    return (decimal)plainArticleField.NativeValue == 1;

                default:
                    return plainArticleField.NativeValue;
            }
        }
    }
}
