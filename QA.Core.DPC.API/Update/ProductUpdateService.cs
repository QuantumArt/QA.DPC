using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Logger;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API.Models;
using Article = QA.Core.Models.Entities.Article;
using Content = QA.Core.Models.Configuration.Content;
using QA.Core.Models;

namespace QA.Core.DPC.API.Update
{
    public class ProductUpdateService : IProductUpdateService
    {
        private readonly IProductService _productService;
        private readonly IArticleService _articleService;
        private readonly IFieldService _fieldService;
        private readonly DeleteAction _deleteAction;
        private readonly ILogger _logger;

        private readonly List<ArticleData> _updateData = new List<ArticleData>();
        private readonly Dictionary<int, Content> _articlesToDelete = new Dictionary<int, Content>();

        private IArticleFilter _filter;

        public ProductUpdateService(
            IProductService productService,
            IArticleService articleService,
            IFieldService fieldService,
            DeleteAction deleteAction,
            ILogger logger)
        {
            _productService = productService;
            _articleService = articleService;
            _fieldService = new CachedFieldService(fieldService);
            _logger = logger;
            _deleteAction = deleteAction;
        }

        #region CachedFieldService

        private class CachedFieldService : IFieldService
        {
            private readonly Dictionary<int, Quantumart.QP8.BLL.Field> _fieldsById
                = new Dictionary<int, Quantumart.QP8.BLL.Field>();

            private readonly Dictionary<int, List<Quantumart.QP8.BLL.Field>> _fieldsByContentId
                = new Dictionary<int, List<Quantumart.QP8.BLL.Field>>();

            private readonly Dictionary<int, List<Quantumart.QP8.BLL.Field>> _relatedFieldsByContentId
                = new Dictionary<int, List<Quantumart.QP8.BLL.Field>>();

            private readonly IFieldService _fieldService;

            public CachedFieldService(IFieldService fieldService)
            {
                _fieldService = fieldService;
            }

            public QPConnectionScope CreateQpConnectionScope()
            {
                return _fieldService.CreateQpConnectionScope();
            }

            public IEnumerable<Quantumart.QP8.BLL.Field> List(int contentId)
            {
                return _fieldsByContentId.ContainsKey(contentId)
                    ? _fieldsByContentId[contentId]
                    : _fieldsByContentId[contentId] = _fieldService
                        .List(contentId)
                        .Select(field => _fieldsById[field.Id] = field)
                        .ToList();
            }

            public IEnumerable<Quantumart.QP8.BLL.Field> ListRelated(int contentId)
            {
                return _relatedFieldsByContentId.ContainsKey(contentId)
                    ? _relatedFieldsByContentId[contentId]
                    : _relatedFieldsByContentId[contentId] = _fieldService
                        .ListRelated(contentId)
                        .Select(field => _fieldsById[field.Id] = field)
                        .ToList();
                
            }

            public Quantumart.QP8.BLL.Field Read(int id)
            {
                return _fieldsById.ContainsKey(id)
                    ? _fieldsById[id]
                    : _fieldsById[id] = _fieldService.Read(id);
            }
        }

        #endregion

        #region IProductUpdateService
        
        // TODO: проверка конфликтов обновления по полю Modified (если оно default(DateTime) - игнорируем)
        /// <exception cref="ProductUpdateConcurrencyException" />
        public InsertData[] Update(
            Article product, ProductDefinition definition, bool isLive = false)
        {
            Article oldProduct = _productService.GetProductById(product.Id, isLive, definition);

            _updateData.Clear();
            _articlesToDelete.Clear();

            _filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ProcessArticlesTree(product, oldProduct, definition.StorageSchema);

            _logger.Debug(() => "Start BatchUpdate : " + ObjectDumper.DumpObject(_updateData));

            InsertData[] idMapping = _articleService.BatchUpdate(_updateData);

            _logger.Debug(() => "End BatchUpdate : " + ObjectDumper.DumpObject(_updateData));
            
            if (_articlesToDelete.Any())
            {
                _logger.Debug(() => "Start Delete : " + ObjectDumper.DumpObject(_articlesToDelete));
                foreach (KeyValuePair<int, Content> articleToDeleteKv in _articlesToDelete)
                {
                    try
                    {
                        var qpArticle = _articleService.Read(articleToDeleteKv.Key);

                        _deleteAction.DeleteProduct(
                            qpArticle, new ProductDefinition { StorageSchema = articleToDeleteKv.Value },
                            true, false, null);
                    }
                    catch (MessageResultException ex)
                    {
                        _logger.ErrorException("Не удалось удалить статью {0}", ex, articleToDeleteKv.Key);
                    }
                }
                _logger.Debug(() => "End Delete : " + ObjectDumper.DumpObject(_articlesToDelete));
            }

            return idMapping;
        }

        #endregion

        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        private void ProcessArticlesTree(Article newArticle, Article existingArticle, Content definition)
        {
            if (newArticle == null || !_filter.Matches(newArticle))
            {
                return;
            }
            if (!_filter.Matches(existingArticle))
            {
                existingArticle = null;
            }
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            ArticleData newArticleUpdateData = new ArticleData
            {
                ContentId = definition.ContentId,
                Id = newArticle.Id
            };

            List<int> plainFieldIds = definition.Fields
                .Where(x => x is PlainField)
                .Select(x => x.FieldId)
                .ToList();

            if (definition.LoadAllPlainFields)
            {
                plainFieldIds.AddRange(
                    _fieldService.List(definition.ContentId)
                        .Where(x => x.RelationType == RelationType.None
                            && definition.Fields.All(y => y.FieldId != x.Id))
                        .Select(x => x.Id));
            }

            var updatedFields = newArticle.Fields.Values
                .OfType<PlainArticleField>()
                .Where(x => plainFieldIds.Contains(x.FieldId.Value)
                    && (existingArticle == null
                        || existingArticle.Fields.Values
                            .OfType<PlainArticleField>()
                            .All(y => y.FieldId != x.FieldId || !HasEqualNativeValues(x, y))))
                .Select(x => new FieldData { Id = x.FieldId.Value, Value = x.Value });

            newArticleUpdateData.Fields.AddRange(updatedFields);

            var associationFieldsInfo = (
                from fieldDef in definition.Fields.OfType<Association>()
                join field in newArticle.Fields.Values on fieldDef.FieldId equals field.FieldId
                select new
                {
                    field,
                    oldField = existingArticle?.Fields.Values.Single(x => x.FieldId == field.FieldId),
                    fieldDef
                }).ToArray();

            foreach (var fieldToSyncInfo in associationFieldsInfo)
            {
                if (fieldToSyncInfo.fieldDef is BackwardRelationField backwardRelationFieldDef)
                {
                    BackwardArticleField oldField = (BackwardArticleField)fieldToSyncInfo.oldField;
                    BackwardArticleField field = (BackwardArticleField)fieldToSyncInfo.field;

                    int[] idsToAdd = field.GetArticles(_filter)
                        .Where(x => oldField == null || oldField.GetArticles(_filter).All(y => y.Id != x.Id))
                        .Select(x => x.Id)
                        .ToArray();

                    int[] idsToRemove = oldField?.GetArticles(_filter)
                        .Select(x => x.Id)
                        .Where(x => field.GetArticles(_filter).All(y => y.Id != x))
                        .ToArray() ?? new int[0];
                    
                    _updateData.AddRange(idsToAdd.Select(x => new ArticleData
                    {
                        Id = x,
                        ContentId = backwardRelationFieldDef.Content.ContentId,
                        Fields = new List<FieldData>
                        {
                            new FieldData { Id = field.FieldId.Value, ArticleIds = new[] { newArticle.Id } }
                        }
                    }));

                    if (backwardRelationFieldDef.DeletingMode == DeletingMode.Delete)
                    {
                        foreach (int idToRemove in idsToRemove)
                        {
                            _articlesToDelete[idToRemove] = backwardRelationFieldDef.Content;
                        }
                    }
                    else
                    {
                        _updateData.AddRange(idsToRemove.Select(x => new ArticleData
                        {
                            Id = x,
                            ContentId = backwardRelationFieldDef.Content.ContentId
                        }));
                    }
                }
                else if (fieldToSyncInfo.fieldDef is ExtensionField extensionFieldDef)
                {
                    ExtensionArticleField oldField = (ExtensionArticleField)fieldToSyncInfo.oldField;
                    ExtensionArticleField field = (ExtensionArticleField)fieldToSyncInfo.field;

                    if (oldField == null || field.Value != oldField.Value)
                    {
                        newArticleUpdateData.Fields.Add(new FieldData
                        {
                            Id = extensionFieldDef.FieldId,
                            Value = field.Value,
                            ArticleIds = field.Item == null ? null : new[] { field.Item.Id }
                        });

                        if (oldField?.Item != null)
                        {
                            _articlesToDelete[oldField.Item.Id] = extensionFieldDef
                                .ContentMapping[oldField.Item.ContentId];
                        }
                    }
                }
                else if (fieldToSyncInfo.field is SingleArticleField)
                {
                    SingleArticleField oldField = (SingleArticleField)fieldToSyncInfo.oldField;
                    SingleArticleField field = (SingleArticleField)fieldToSyncInfo.field;
                    EntityField entityFieldDef = (EntityField)fieldToSyncInfo.fieldDef;

                    Article item = field.GetItem(_filter);

                    Article oldItem = oldField?.GetItem(_filter);

                    if (item?.Id != oldItem?.Id)
                    {
                        newArticleUpdateData.Fields.Add(new FieldData
                        {
                            Id = field.FieldId.Value,
                            ArticleIds = item == null ? null : new[] { item.Id }
                        });

                        if (oldItem != null && entityFieldDef.DeletingMode == DeletingMode.Delete)
                        {
                            _articlesToDelete[oldItem.Id] = entityFieldDef.Content;
                        }
                    }
                }
                else if (fieldToSyncInfo.field is MultiArticleField)
                {
                    MultiArticleField oldField = (MultiArticleField)fieldToSyncInfo.oldField;
                    MultiArticleField field = (MultiArticleField)fieldToSyncInfo.field;
                    EntityField entityFieldDef = (EntityField)fieldToSyncInfo.fieldDef;

                    var items = field.GetArticles(_filter).ToArray();

                    var oldItems = oldField?.GetArticles(_filter).ToArray();

                    if (items.Length != (oldItems?.Length ?? 0) || items.Any(x => oldItems.All(y => y.Id != x.Id)))
                    {
                        newArticleUpdateData.Fields.Add(new FieldData
                        {
                            Id = field.FieldId.Value, ArticleIds = items.Select(x => x.Id).ToArray()
                        });

                        if (entityFieldDef.DeletingMode == DeletingMode.Delete)
                        {
                            int[] idsToRemove = oldItems?
                                .Where(x => items.All(y => y.Id != x.Id))
                                .Select(x => x.Id)
                                .ToArray() ?? new int[0];

                            foreach (int idToRemove in idsToRemove)
                            {
                                _articlesToDelete[idToRemove] = entityFieldDef.Content;
                            }
                        }
                    }
                }
            }

            // if (newArticleUpdateData.Fields.Any())
            _updateData.Add(newArticleUpdateData);

            foreach (var fieldInfo in associationFieldsInfo
                .Where(x => x.fieldDef.UpdatingMode == UpdatingMode.Update))
            {
                Article[] oldFieldsArticles = fieldInfo.oldField == null
                    ? new Article[0]
                    : GetChildArticles(fieldInfo.oldField, _filter).ToArray();

                foreach (Article childArticle in GetChildArticles(fieldInfo.field, _filter))
                {
                    Content childArticleDef = fieldInfo.fieldDef.Contents
                        .SingleOrDefault(x => x.ContentId == childArticle.ContentId);

                    if (childArticleDef == null)
                    {
                        throw new InvalidOperationException($@"В описании продукта у поля {fieldInfo.field.FieldId
                            } не может быть ContentId={childArticle.ContentId
                            }, который у статьи id={childArticle.Id}");
                    }

                    Article oldChildArticle = oldFieldsArticles.SingleOrDefault(x => x.Id == childArticle.Id);

                    ProcessArticlesTree(childArticle, oldChildArticle, childArticleDef);
                }
            }
        }

        private static bool HasEqualNativeValues(
            PlainArticleField plainArticleField, PlainArticleField otherPlainArticleField)
        {
            return plainArticleField.NativeValue == null
                    ? otherPlainArticleField.NativeValue == null
                    : plainArticleField.NativeValue.Equals(otherPlainArticleField.NativeValue);
        }

        private IEnumerable<Article> GetChildArticles(ArticleField field, IArticleFilter filter)
        {
            if (field is IGetArticle getArticle)
            {
                Article childArticle = getArticle.GetItem(filter);

                return childArticle != null ? new[] { childArticle } : new Article[0];
            }
            else if (field is IGetArticles getArticles)
            {
                return getArticles.GetArticles(filter);
            }
            else
            {
                return new Article[0];
            }
        }
    }
}