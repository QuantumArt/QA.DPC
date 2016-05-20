using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using QA.Core.DPC.Loader.Services;
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
        #region Private properties
        private readonly IProductService _productService;
        private readonly IArticleService _articleService;
        private readonly ILogger _logger;

        private readonly List<ArticleData> _updateData = new List<ArticleData>();
        private readonly Dictionary<int, Content> _articlesToDelete = new Dictionary<int, Content>();
        private readonly IFieldService _fieldService;
        private readonly DeleteAction _deleteAction;

        private IArticleFilter _filter;

        #endregion

        #region Constructor
        public ProductUpdateService(IProductService productService, IArticleService articleService, ILogger logger, IFieldService fieldService, DeleteAction deleteAction)
        {
            _productService = productService;
            _articleService = articleService;
            _logger = logger;
            _fieldService = fieldService;
            _deleteAction = deleteAction;
        }
        #endregion

        #region IProductUpdateService implementation

        public void Update(Article product, ProductDefinition definition, bool isLive = false)
        {
            var oldProduct = _productService.GetProductById(product.Id, isLive, definition);

            _updateData.Clear();

            _articlesToDelete.Clear();

            _filter = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

            ProcessArticlesTree(product, oldProduct, definition.StorageSchema);

            _logger.Debug(_ => "Start BatchUpdate : " + ObjectDumper.DumpObject(_updateData));
            _articleService.BatchUpdate(_updateData);
            _logger.Debug(_ => "End BatchUpdate : " + ObjectDumper.DumpObject(_updateData));


            if (_articlesToDelete.Any())
            {
                _logger.Debug(_ => "Start Delete : " + ObjectDumper.DumpObject(_articlesToDelete));

                foreach (KeyValuePair<int, Content> articleToDeleteKv in _articlesToDelete)
                {
                    try
                    {
                        var qpArticle = _articleService.Read(articleToDeleteKv.Key);

                        _deleteAction.DeleteProduct(qpArticle, new ProductDefinition { StorageSchema = articleToDeleteKv.Value }, true, false);
                    }
                    catch (MessageResultException ex)
                    {
                        _logger.ErrorException("Не удалось удалить статью {0}", ex, articleToDeleteKv.Key);
                    }
                }
            }

            _logger.Debug(_ => "End Delete : " + ObjectDumper.DumpObject(_articlesToDelete));

        }
        #endregion

        #region Private methods



        private void ProcessArticlesTree(Article newArticle, Article existingArticle, Content definition)
        {
            if (newArticle == null || !_filter.Matches(newArticle))
                return;

            if (!_filter.Matches(existingArticle))
                existingArticle = null;

            if (definition == null)
                throw new ArgumentNullException("definition");

            ArticleData newArticleUpdateData = new ArticleData
            {
                ContentId = definition.ContentId,
                Id = newArticle.Id
            };

            List<int> plainFieldIds = definition.Fields.Where(x => x is PlainField).Select(x => x.FieldId).ToList();

            if (definition.LoadAllPlainFields)
            {
                plainFieldIds.AddRange(
                    _fieldService.List(definition.ContentId)
                        .Where(x => x.RelationType == RelationType.None && definition.Fields.All(y => y.FieldId != x.Id))
                        .Select(x => x.Id));
            }

            newArticleUpdateData.Fields.AddRange(
                newArticle.Fields.Values.OfType<PlainArticleField>()
                    .Where(x => plainFieldIds.Contains(x.FieldId.Value) && (existingArticle == null || existingArticle.Fields.Values.OfType<PlainArticleField>()
                                                                                                       .All(y => y.FieldId != x.FieldId || !ComparePlainFields(x, y))))
                    .Select(x => new FieldData { Id = x.FieldId.Value, Value = x.Value }));

            var associationFieldsInfo = (
                from fieldDef in definition.Fields.OfType<Association>()
                join field in newArticle.Fields.Values on fieldDef.FieldId equals field.FieldId
                select
                    new
                    {
                        field,
                        oldField = existingArticle?.Fields.Values.Single(x => x.FieldId == field.FieldId),
                        fieldDef
                    }).ToArray();

            foreach (var fieldToSyncInfo in associationFieldsInfo)
            {
                if (fieldToSyncInfo.fieldDef is BackwardRelationField)
                {
                    BackwardArticleField oldField = (BackwardArticleField)fieldToSyncInfo.oldField, field = (BackwardArticleField)fieldToSyncInfo.field;

                    int[] idsToAdd =
                        field.GetArticles(_filter)
                            .Where(x => oldField == null || oldField.GetArticles(_filter).All(y => y.Id != x.Id))
                            .Select(x => x.Id)
                            .ToArray();

                    int[] idsToRemove = oldField?.GetArticles(_filter).Select(x => x.Id).Where(x => field.GetArticles(_filter).All(y => y.Id != x)).ToArray() ?? new int[] { };

                    BackwardRelationField backwardRelationFieldDef = (BackwardRelationField)fieldToSyncInfo.fieldDef;

                    _updateData.AddRange(idsToAdd.Select(x => new ArticleData
                    {
                        Id = x,
                        ContentId = backwardRelationFieldDef.Content.ContentId,
                        Fields =
                            new List<FieldData>
                            {
                                new FieldData {Id = field.FieldId.Value, ArticleIds = new[] {newArticle.Id}}
                            }
                    }));

                    if (fieldToSyncInfo.fieldDef.DeletingMode == DeletingMode.Delete)
                    {
                        foreach (int idToRemove in idsToRemove)
                        {
                            _articlesToDelete[idToRemove] = backwardRelationFieldDef.Content;
                        }
                    }
                    else
                        _updateData.AddRange(idsToRemove.Select(x => new ArticleData
                        {
                            Id = x,
                            ContentId = backwardRelationFieldDef.Content.ContentId
                        }));
                }
                else if (fieldToSyncInfo.fieldDef is ExtensionField)
                {
                    ExtensionArticleField oldField = (ExtensionArticleField)fieldToSyncInfo.oldField, field = (ExtensionArticleField)fieldToSyncInfo.field;

                    if (oldField == null || field.Value != oldField.Value)
                    {
                        newArticleUpdateData.Fields.Add(new FieldData
                        {
                            Id = fieldToSyncInfo.fieldDef.FieldId,
                            Value = field.Value,
                            ArticleIds = field.Item == null ? null : new[] { field.Item.Id }
                        });

                        if (oldField?.Item != null)
                            _articlesToDelete[oldField.Item.Id] = ((ExtensionField)fieldToSyncInfo.fieldDef).ContentMapping[oldField.Item.ContentId];
                    }
                }
                else if (fieldToSyncInfo.field is SingleArticleField)
                {
                    SingleArticleField oldField = (SingleArticleField)fieldToSyncInfo.oldField, field = (SingleArticleField)fieldToSyncInfo.field;

                    Article item = field.GetItem(_filter);

                    Article oldItem = oldField?.GetItem(_filter);

                    if (item?.Id != oldItem?.Id)
                    {
                        newArticleUpdateData.Fields.Add(new FieldData
                        {
                            Id = field.FieldId.Value,
                            ArticleIds = item == null ? null : new[] { item.Id }
                        });

                        if (oldItem != null && fieldToSyncInfo.fieldDef.DeletingMode == DeletingMode.Delete)
                            _articlesToDelete[oldItem.Id] = ((EntityField)fieldToSyncInfo.fieldDef).Content;
                    }
                }
                else if (fieldToSyncInfo.field is MultiArticleField)
                {
                    MultiArticleField oldField = (MultiArticleField)fieldToSyncInfo.oldField, field = (MultiArticleField)fieldToSyncInfo.field;

                    var items = field.GetArticles(_filter).ToArray();

                    var oldItems = oldField?.GetArticles(_filter).ToArray();

                    if (items.Length != (oldItems?.Length ?? 0) || items.Any(x => oldItems.All(y => y.Id != x.Id)))
                    {
                        newArticleUpdateData.Fields.Add(new FieldData { Id = field.FieldId.Value, ArticleIds = items.Select(x => x.Id).ToArray() });

                        if (fieldToSyncInfo.fieldDef.DeletingMode == DeletingMode.Delete)
                        {
                            int[] idsToRemove = oldItems?.Where(x => items.All(y => y.Id != x.Id)).Select(x => x.Id).ToArray() ?? new int[] { };

                            foreach (int idToRemove in idsToRemove)
                            {
                                _articlesToDelete[idToRemove] = ((EntityField)fieldToSyncInfo.fieldDef).Content;
                            }
                        }
                    }
                }
            }

            //if (newArticleUpdateData.Fields.Any())
            _updateData.Add(newArticleUpdateData);

            foreach (var fieldInfo in associationFieldsInfo.Where(x => x.fieldDef.UpdatingMode == UpdatingMode.Update))
            {
                Article[] oldFieldsArticles = fieldInfo.oldField == null
                    ? new Article[] { }
                    : GetChildArticles(fieldInfo.oldField, _filter).ToArray();

                foreach (Article childArticle in GetChildArticles(fieldInfo.field, _filter))
                {
                    Content childArticleDef = fieldInfo.fieldDef.Contents.SingleOrDefault(x => x.ContentId == childArticle.ContentId);

                    if (childArticleDef == null)
                        throw new Exception(string.Format("В описании продукта у поля {0} не может быть ContentId={1}, который у статьи id={2}",
                                fieldInfo.field.FieldId, childArticle.ContentId, childArticle.Id));

                    ProcessArticlesTree(childArticle, oldFieldsArticles.SingleOrDefault(x => x.Id == childArticle.Id), childArticleDef);
                }
            }
        }

        private static bool ComparePlainFields(PlainArticleField plainArticleField, PlainArticleField otherPlainArticleField)
        {
            return plainArticleField.NativeValue == null && otherPlainArticleField.NativeValue == null ||
                   plainArticleField.NativeValue != null && plainArticleField.NativeValue.Equals(otherPlainArticleField.NativeValue);
        }

        private IEnumerable<Article> GetChildArticles(ArticleField field, IArticleFilter filter)
        {
            IEnumerable<Article> childArticles;

            if (field is IGetArticle)
            {
                var childArticle = ((IGetArticle)field).GetItem(filter);

                childArticles = childArticle != null ? new[] { childArticle } : new Article[] { };
            }
            else if (field is IGetArticles)
            {
                childArticles = ((IGetArticles)field).GetArticles(filter);
            }
            else childArticles = new Article[] { };

            return childArticles;
        }

        #endregion
    }
}