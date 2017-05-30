using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Article = QA.Core.Models.Entities.Article;
using Field = QA.Core.Models.Configuration.Field;
using Quantumart.QPublishing.Database;
using QA.Core.DPC.QP.Servives;

namespace QA.Core.DPC.Loader
{
    internal interface IProductDeserializer
    {
        Article Deserialize(IProductDataSource productDataSource, Models.Configuration.Content definition);
    }

    internal class ProductDeserializer : IProductDeserializer
    {
        private readonly IFieldService _fieldService;
        private readonly ContentService _contentService;
        private readonly ICacheItemWatcher _cacheItemWatcher;
        private readonly IContextStorage _contextStorage;
        private readonly string _connectionString;

        public ProductDeserializer(IFieldService fieldService, IServiceFactory serviceFactory, ICacheItemWatcher cacheItemWatcher, IContextStorage contextStorage, IConnectionProvider connectionProvider)
        {
            _fieldService = fieldService;

            _contentService = serviceFactory.GetContentService();

            _cacheItemWatcher = cacheItemWatcher;

            _contextStorage = contextStorage;

            _connectionString = connectionProvider.GetConnection();
        }
       

        public Article Deserialize(IProductDataSource productDataSource, Models.Configuration.Content definition)
        {
            using (var cs = new QPConnectionScope(_connectionString))
            {
                _cacheItemWatcher.TrackChanges();

                _contentService.LoadStructureCache(_contextStorage);

                var connector = new DBConnector(cs.DbConnection);

                return DeserializeArticle(productDataSource, definition, connector);
            }
        }

        private Article DeserializeArticle(IProductDataSource productDataSource, Models.Configuration.Content definition, DBConnector connector)
        {
            if (productDataSource == null)
                return null;

            int? id = productDataSource.GetInt("Id");

            var qpContent = _contentService.Read(definition.ContentId);

            Article article = new Article
            {
                ContentId = definition.ContentId,
                ContentDisplayName = qpContent.Name,
                PublishingMode = definition.PublishingMode,
                ContentName = qpContent.NetName,
                Id = id ?? default(int),
                Visible = true
            };

            foreach (Field fieldInDef in definition.Fields.Where(x => !(x is BaseVirtualField) && !(x is Dictionaries)))
            {
                var field = DeserializeField(fieldInDef, _fieldService.Read(fieldInDef.FieldId), productDataSource, connector);

                article.Fields[field.FieldName] = field;
            }

            if (definition.LoadAllPlainFields)
            {
                var qpFields = _fieldService.List(definition.ContentId);

                foreach (var plainFieldFromQp in qpFields.Where(x => x.RelationType == RelationType.None && definition.Fields.All(y => y.FieldId != x.Id)))
                {
                    article.Fields[plainFieldFromQp.Name] = DeserializeField(new PlainField { FieldId = plainFieldFromQp.Id }, plainFieldFromQp, productDataSource, connector);
                }
            }

            return article;
        }

        private ArticleField DeserializeField(Field fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector)
        {
            ArticleField field;

            if (fieldInDef is BackwardRelationField)
                field = DeserializeBackwardField((BackwardRelationField)fieldInDef, productDataSource, connector);
            else if (fieldInDef is EntityField)
                field = DeserializeEntityField((EntityField)fieldInDef, qpField, productDataSource, connector);
            else if (fieldInDef is ExtensionField)
                field = DeserializeExtensionField((ExtensionField)fieldInDef, qpField, productDataSource, connector);
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
                PlainFieldType = (PlainFieldType)plainFieldFromQP.ExactType
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
                        object number = plainFieldFromQP.IsInteger ||
                                        plainFieldFromQP.RelationType == RelationType.OneToMany
                            ? productDataSource.GetInt(plainFieldFromQP.Name)
                            : (object)productDataSource.GetDecimal(plainFieldFromQP.Name);


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

                    IProductDataSource fileContainer = productDataSource.GetContainer(plainFieldFromQP.Name);

                    if (fileContainer != null)
                    {
                        string fileUrl = fileContainer.GetString("AbsoluteUrl");
                        string fileName = Common.GetFileNameByUrl(connector, plainFieldFromQP.Id, fileUrl);

                        field.NativeValue = field.Value = fileName;
                    }

                    break;

                case PlainFieldType.Boolean:
                    {
                        decimal? number = productDataSource.GetDecimal(plainFieldFromQP.Name);

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

        private ArticleField DeserializeExtensionField(ExtensionField fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector)
        {
            var extensionArticleField = new ExtensionArticleField();

            string fieldName = string.IsNullOrEmpty(fieldInDef.FieldName) ? qpField.Name : fieldInDef.FieldName;

            string contentName = productDataSource.GetString(fieldName);

            if (!string.IsNullOrEmpty(contentName))
            {
                Models.Configuration.Content valueDef = fieldInDef.ContentMapping.Values.FirstOrDefault(x => _contentService.Read(x.ContentId).NetName == contentName);

                if (valueDef == null)
                    throw new Exception(string.Format("Значение '{0}' не найдено в списке допустимых контентов ExtensionField id = {1}", contentName, fieldInDef.FieldId));

                extensionArticleField.Value = valueDef.ContentId.ToString();

                extensionArticleField.Item = DeserializeArticle(productDataSource, valueDef, connector);

                extensionArticleField.Item.Id = default(int);

                extensionArticleField.SubContentId = valueDef.ContentId;
            }

            return extensionArticleField;
        }

        private ArticleField DeserializeBackwardField(BackwardRelationField fieldInDef, IProductDataSource productDataSource, DBConnector connector)
        {
            if (string.IsNullOrEmpty(fieldInDef.FieldName))
                throw new Exception("В описании BackwardArticleField должен быть непустой FieldName");

            IEnumerable<IProductDataSource> containersCollection = productDataSource.GetContainersCollection(fieldInDef.FieldName);

            var backwardArticleField = new BackwardArticleField
            {
                SubContentId = fieldInDef.Content.ContentId,
            };

            if (containersCollection != null)
                foreach (Article article in containersCollection.Select(x => DeserializeArticle(x, fieldInDef.Content, connector)))
                    backwardArticleField.Items.Add(article.Id, article);

            return backwardArticleField;
        }

        private ArticleField DeserializeEntityField(EntityField fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector)
        {
            string fieldName = fieldInDef.FieldName ?? qpField.Name;

            ArticleField articleField;

            if (qpField.RelationType == RelationType.OneToMany)
            {
                articleField = new SingleArticleField
                {
                    Item = DeserializeArticle(productDataSource.GetContainer(fieldName), fieldInDef.Content, connector),
                    Aggregated = qpField.Aggregated,
                    SubContentId = fieldInDef.Content.ContentId
                };
            }
            else if (qpField.RelationType == RelationType.ManyToMany || qpField.RelationType == RelationType.ManyToOne)
            {
                var multiArticleField = new MultiArticleField { SubContentId = fieldInDef.Content.ContentId };

                var containersCollection = productDataSource.GetContainersCollection(fieldName);

                if (containersCollection != null)
                    foreach (Article article in containersCollection.Select(x => DeserializeArticle(x, fieldInDef.Content, connector)))
                        multiArticleField.Items.Add(article.Id, article);

                articleField = multiArticleField;
            }
            else throw new Exception(string.Format("В описании поле id={0} имеет тип EntityField но его RelationType не соответствует требуемым", fieldInDef.FieldId));

            return articleField;
        }
    }
}
