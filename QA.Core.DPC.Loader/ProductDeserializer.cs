﻿using QA.Core.Cache;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using QP.ConfigurationService.Models;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository.ArticleMatching.Conditions;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Article = QA.Core.Models.Entities.Article;
using Field = QA.Core.Models.Configuration.Field;

namespace QA.Core.DPC.Loader
{
    public class ProductDeserializer : IProductDeserializer
    {
        private readonly IFieldService _fieldService;
        protected readonly ContentService _contentService;
        private readonly ICacheItemWatcher _cacheItemWatcher;
        private readonly IContextStorage _contextStorage;
        private readonly Customer _customer;

        private string GetExstensionIdQuery(DatabaseType dbType) => $@"
            select ATTRIBUTE_NAME from CONTENT_ATTRIBUTE
            where
                CLASSIFIER_ATTRIBUTE_ID = @attributeId and
                CONTENT_ID = @contentId and
                AGGREGATED = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";

        public ProductDeserializer(
            IFieldService fieldService,
            IServiceFactory serviceFactory,
            ICacheItemWatcher cacheItemWatcher,
            IContextStorage contextStorage,
            IConnectionProvider connectionProvider)
        {
            _fieldService = fieldService;

            _contentService = serviceFactory.GetContentService();

            _cacheItemWatcher = cacheItemWatcher;

            _contextStorage = contextStorage;

            _customer = connectionProvider.GetCustomer();
        }

        public Article Deserialize(IProductDataSource productDataSource, Models.Configuration.Content definition)
        {
            using (var cs = new QPConnectionScope(_customer.ConnectionString, (Quantumart.QP8.Constants.DatabaseType)_customer.DatabaseType))
            {
                _cacheItemWatcher.TrackChanges();

                _contentService.LoadStructureCache(_contextStorage);
                var connector = new DBConnector(cs.DbConnection);
                var context = new Context();
                var article = DeserializeArticle(productDataSource, definition, connector, context);
                context.UpdateExtensionArticles();
                return article;
            }
        }

        protected virtual Article DeserializeArticle(
            IProductDataSource productDataSource,
            Models.Configuration.Content definition,
            DBConnector connector,
            Context context)
        {
            if (productDataSource == null)
                return null;

            int id = GetArticleId(productDataSource, definition.ContentId);

            context.TakeIntoAccount(id);

            var qpContent = _contentService.Read(definition.ContentId);

            Article article = new Article
            {
                Id = id,
                ContentName = qpContent.NetName,
                Modified = productDataSource.GetModified(),
                ContentId = definition.ContentId,
                ContentDisplayName = qpContent.Name,
                PublishingMode = definition.PublishingMode,
                IsReadOnly = definition.IsReadOnly,
                Visible = true
            };

            foreach (Field fieldInDef in definition.Fields.Where(x => !(x is BaseVirtualField) && !(x is Dictionaries)))
            {
                var field = DeserializeField(fieldInDef, _fieldService.Read(fieldInDef.FieldId), productDataSource, connector, context);
                ApplyArticleField(article, field, productDataSource);
            }

            if (definition.LoadAllPlainFields)
            {
                var qpFields = _fieldService.List(definition.ContentId);

                foreach (var plainFieldFromQp in qpFields.Where(x => x.RelationType == RelationType.None && definition.Fields.All(y => y.FieldId != x.Id)))
                {
                    PlainField fieldInDefinition = new PlainField
                    {
                        FieldId = plainFieldFromQp.Id
                    };

                    ArticleField articleField = DeserializeField(fieldInDefinition, plainFieldFromQp, productDataSource, connector, context);
                    ApplyArticleField(article, articleField, productDataSource);
                }
            }

            return article;
        }

        protected virtual void ApplyArticleField(Article article, ArticleField field, IProductDataSource productDataSource)
        {
            article.Fields[field.FieldName] = field;
        }

        protected virtual int GetArticleId(IProductDataSource productDataSource, int contentId)
        {
            var articleId = productDataSource.GetArticleId();

            return articleId > 0 ? articleId : default;
        }

        private ArticleField DeserializeField(Field fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector, Context context)
        {
            ArticleField field;

            if (fieldInDef is BackwardRelationField)
                field = DeserializeBackwardField((BackwardRelationField)fieldInDef, productDataSource, connector, context);
            else if (fieldInDef is EntityField)
                field = DeserializeEntityField((EntityField)fieldInDef, qpField, productDataSource, connector, context);
            else if (fieldInDef is ExtensionField)
                field = DeserializeExtensionField((ExtensionField)fieldInDef, qpField, productDataSource, connector, context);
            else if (fieldInDef is PlainField)
                field = DeserializePlainField(qpField, productDataSource, connector);
            else throw new Exception("Неподдерживаемый тип поля: " + fieldInDef.GetType().Name);

            field.ContentId = qpField.ContentId;

            field.FieldId = qpField.Id;

            field.FieldName = string.IsNullOrEmpty(fieldInDef.FieldName) ? qpField.Name : fieldInDef.FieldName;

            field.FieldDisplayName = qpField.DisplayName;

            return field;
        }

        private ArticleField DeserializePlainField(Quantumart.QP8.BLL.Field plainFieldFromQP, IProductDataSource productDataSource, DBConnector connector)
        {
            var field = new PlainArticleField
            {
                PlainFieldType = (PlainFieldType)plainFieldFromQP.ExactType,
                DefaultValue = plainFieldFromQP.DefaultValue
            };

            switch (field.PlainFieldType)
            {
                case PlainFieldType.Date:
                case PlainFieldType.DateTime:

                    DateTime? dt = productDataSource.GetDateTime(plainFieldFromQP.Name);

                    if (dt.HasValue)
                    {
                        field.Value = dt.ToString();

                        field.NativeValue = dt;
                    }
                    break;

                case PlainFieldType.Numeric:
                case PlainFieldType.O2MRelation:
                    {
                        object number;
                        if (plainFieldFromQP.IsInteger ||
                            plainFieldFromQP.RelationType == RelationType.OneToMany)
                        {
                            number = productDataSource.GetInt(plainFieldFromQP.Name);
                        }
                        else
                        {
                            var dec = productDataSource.GetDecimal(plainFieldFromQP.Name);
                            number = !plainFieldFromQP.IsDecimal && dec.HasValue ? (object) Convert.ToDouble(dec) : dec;
                        }

                        if (number != null)
                        {
                            field.Value = number.ToString();

                            field.NativeValue = number;
                        }
                    }
                    break;

                case PlainFieldType.Image:
                    string imageUrl = productDataSource.GetString(plainFieldFromQP.Name);

                    if (imageUrl != null)
                    {
                        string imageName = Common.GetFileNameByUrl(connector, plainFieldFromQP.Id, imageUrl);
                        field.NativeValue = field.Value = imageName;
                    }
                    break;

                case PlainFieldType.File:
                    if (productDataSource is EditorJsonProductDataSource)
                    {
                        string fileName = productDataSource.GetString(plainFieldFromQP.Name);
                        field.NativeValue = field.Value = fileName;
                    }
                    else
                    {
                        IProductDataSource fileContainer = productDataSource.GetContainer(plainFieldFromQP.Name);

                        if (fileContainer != null)
                        {
                            string fileUrl = fileContainer.GetString("AbsoluteUrl");
                            string fileName = Common.GetFileNameByUrl(connector, plainFieldFromQP.Id, fileUrl);
                            field.NativeValue = field.Value = fileName;
                        }
                    }
                    break;

                case PlainFieldType.Boolean:
                    {
                        decimal? number = productDataSource.GetBoolAsDecimal(plainFieldFromQP.Name);

                        if (number != null)
                        {
                            field.Value = number.ToString();

                            field.NativeValue = number;
                        }
                    }
                    break;

                default:
                    field.NativeValue = field.Value = productDataSource.GetString(plainFieldFromQP.Name);
                    break;
            }

            field.Value = field.Value ?? string.Empty;

            return field;
        }

        private ArticleField DeserializeExtensionField(ExtensionField fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector, Context context)
        {
            var extensionArticleField = new ExtensionArticleField();

            string fieldName = string.IsNullOrEmpty(fieldInDef.FieldName) ? qpField.Name : fieldInDef.FieldName;

            string contentName = productDataSource.GetString(fieldName);

            if (!string.IsNullOrEmpty(contentName))
            {
                Models.Configuration.Content valueDef = fieldInDef.ContentMapping.Values.FirstOrDefault(x => _contentService.Read(x.ContentId).NetName == contentName);

                if (valueDef == null)
                    throw new Exception(
                        $"'{contentName}' value is not found in an available extension content list, id = {fieldInDef.FieldId}");

                extensionArticleField.Value = valueDef.ContentId.ToString();
                extensionArticleField.SubContentId = valueDef.ContentId;

                IProductDataSource extensionDataSource = productDataSource.GetExtensionContainer(fieldName, contentName);

                extensionArticleField.Item = DeserializeArticle(extensionDataSource, valueDef, connector, context);

                if (extensionArticleField.Item.Id == productDataSource.GetArticleId())
                {
                    var id = GetExtensionId(connector, valueDef.ContentId, fieldInDef.FieldId, extensionArticleField.Item.Id);

                    if (id.HasValue)
                    {
                        extensionArticleField.Item.Id = id.Value;
                    }
                    else
                    {
                        context.AddExtensionArticle(extensionArticleField.Item.Id, extensionArticleField.Item);
                        extensionArticleField.Item.Id = default(int);
                    }
                }
            }

            return extensionArticleField;
        }

        private ArticleField DeserializeBackwardField(BackwardRelationField fieldInDef, IProductDataSource productDataSource, DBConnector connector, Context context)
        {
            if (string.IsNullOrEmpty(fieldInDef.FieldName))
                throw new Exception("BackwardArticleField definition should have non-empty FieldName");

            IEnumerable<IProductDataSource> containersCollection = productDataSource.GetContainersCollection(fieldInDef.FieldName);

            var backwardArticleField = new BackwardArticleField
            {
                SubContentId = fieldInDef.Content.ContentId,
            };

            if (containersCollection != null)
                foreach (Article article in containersCollection.Select(x => DeserializeArticle(x, fieldInDef.Content, connector, context)))
                    backwardArticleField.Items.Add(article.Id, article);

            return backwardArticleField;
        }

        private ArticleField DeserializeEntityField(EntityField fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector, Context context)
        {
            string fieldName = fieldInDef.FieldName ?? qpField.Name;

            ArticleField articleField;

            if (qpField.RelationType == RelationType.OneToMany)
            {
                articleField = new SingleArticleField
                {
                    Item = DeserializeArticle(productDataSource.GetContainer(fieldName), fieldInDef.Content, connector, context),
                    Aggregated = qpField.Aggregated,
                    SubContentId = fieldInDef.Content.ContentId
                };
            }
            else if (qpField.RelationType == RelationType.ManyToMany || qpField.RelationType == RelationType.ManyToOne)
            {
                var multiArticleField = new MultiArticleField { SubContentId = fieldInDef.Content.ContentId };

                var containersCollection = productDataSource.GetContainersCollection(fieldName);

                if (containersCollection != null)
                    foreach (Article article in containersCollection.Select(x => DeserializeArticle(x, fieldInDef.Content, connector, context)))
                        multiArticleField.Items.Add(article.Id, article);

                articleField = multiArticleField;
            }
            else throw new Exception(string.Format("Field definition id={0} has EntityField type but its RelationType is not valid", fieldInDef.FieldId));

            return articleField;
        }

        private int? GetExtensionId(DBConnector connector, int contentId, int attributeId, int articleId)
        {
            var dbCommand = connector.CreateDbCommand(GetExstensionIdQuery(connector.DatabaseType));

            dbCommand.Parameters.AddWithValue("@contentId", contentId);
            dbCommand.Parameters.AddWithValue("@attributeId", attributeId);
            var dt = connector.GetRealData(dbCommand);

            var attributes =  dt.AsEnumerable().Select(x => x["ATTRIBUTE_NAME"]).ToArray();
            var queries = attributes
                .Select(a =>
                    $"select CONTENT_ITEM_ID id from content_{contentId}_united where {a} = @articleId");

            dbCommand = connector.CreateDbCommand(string.Join(" ", queries));
            dbCommand.Parameters.AddWithValue("@articleId", articleId);

            var value = connector.GetRealScalarData(dbCommand);

            return (int?)(decimal?)value;
        }
    }
}
