using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API;
using System;
using System.Linq;

namespace QA.Core.DPC.Loader.Editor
{
    public class EditorDataService
    {
        private readonly ContentService _contentService;

        public EditorDataService(ContentService contentService)
        {
            _contentService = contentService;
        }
        
        private class ArticleContext
        {
            public IArticleFilter Filter;

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
                    case PlainArticleField plainField:
                        return plainField.PlainFieldType != PlainFieldType.DynamicImage;
                    default:
                        return true;
                }
            }
        }

        /// <exception cref="InvalidOperationException" />
        /// <exception cref="NotSupportedException" />
        public ArticleObject ConvertArticle(Article article, IArticleFilter filter)
        {
            _contentService.LoadStructureCache();

            return ConvertArticle(article, new ArticleContext
            {
                Filter = filter,
            });
        }

        /// <exception cref="InvalidOperationException" />
        /// <exception cref="NotSupportedException" />
        private ArticleObject ConvertArticle(Article article, ArticleContext context, bool forExtension = false)
        {
            if (!context.ShouldIncludeArticle(article))
            {
                return null;
            }

            var dict = new ArticleObject();

            if (forExtension)
            {
                dict[ArticleObject.ContentName] = article.ContentName;
            }
            else
            {
                dict[ArticleObject.Id] = article.Id;
                dict[ArticleObject.ContentName] = article.ContentName;
                dict[ArticleObject.Timestamp] = article.Modified == default(DateTime)
                    ? article.Created : article.Modified;
            }

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
        private void PopulateField(ArticleObject dict, ArticleField field, ArticleContext context)
        {
            if (field is ExtensionArticleField extensionArticleField)
            {
                PopulateExtensionFields(dict, extensionArticleField, context);
            }
            else if (field is MultiArticleField multiArticleField)
            {
                dict[field.FieldName] = multiArticleField
                    .GetArticles(context.Filter)
                    .Select(f => ConvertArticle(f, context))
                    .Where(a => a != null)
                    .ToArray();
            }
            else if (field is SingleArticleField singleArticleField)
            {
                dict[field.FieldName] = ConvertArticle(singleArticleField.GetItem(context.Filter), context);
            }
            else if (field is PlainArticleField plainArticleField)
            {
                dict[field.FieldName] = ConvertPlainField(plainArticleField);
            }
            else
            {
                throw new NotSupportedException($"Поле типа {field.GetType()} не поддерживается");
            }
        }

        /// <exception cref="InvalidOperationException" />
        private void PopulateExtensionFields(
            ArticleObject dict, ExtensionArticleField field, ArticleContext context)
        {
            if (field.Item == null)
            {
                return;
            }

            Article article = field.Item;

            dict[field.FieldName] = article.ContentName;
            dict[ArticleObject.Contents(field.FieldName)] = new ExtensionFieldObject
            {
                [article.ContentName] = ConvertArticle(article, context, forExtension: true)
            };
        }

        private object ConvertPlainField(PlainArticleField plainArticleField)
        {
            if (plainArticleField.NativeValue == null)
            {
                return null;
            }

            switch (plainArticleField.PlainFieldType)
            {
                case PlainFieldType.Boolean:
                    return (decimal)plainArticleField.NativeValue == 1;

                case PlainFieldType.O2MRelation:
                    return (int)plainArticleField.NativeValue;

                case PlainFieldType.Classifier:
                    return _contentService.Read((int)(decimal)plainArticleField.NativeValue).NetName;

                default:
                    return plainArticleField.NativeValue;
            }
        }
    }
}
