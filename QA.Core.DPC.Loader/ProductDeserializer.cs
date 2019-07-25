using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Services;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.API;
using Article = QA.Core.Models.Entities.Article;
using Field = QA.Core.Models.Configuration.Field;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;

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
        private readonly Customer _customer;

        private string GetExstensionIdQuery(DatabaseType dbType) => $@"
            select ATTRIBUTE_NAME from CONTENT_ATTRIBUTE
            where
	            CLASSIFIER_ATTRIBUTE_ID = @attributeId and
	            CONTENT_ID = @contentId and
	            AGGREGATED = {SqlQuerySyntaxHelper.ToBoolSql(dbType, true)}";

        public ProductDeserializer(IFieldService fieldService, IServiceFactory serviceFactory, ICacheItemWatcher cacheItemWatcher, IContextStorage contextStorage, IConnectionProvider connectionProvider)
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

        private Article DeserializeArticle(IProductDataSource productDataSource, Models.Configuration.Content definition, DBConnector connector, Context context)
        {
            if (productDataSource == null)
                return null;

            int id = productDataSource.GetArticleId();

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

                article.Fields[field.FieldName] = field;
            }

            if (definition.LoadAllPlainFields)
            {
                var qpFields = _fieldService.List(definition.ContentId);

                foreach (var plainFieldFromQp in qpFields.Where(x => x.RelationType == RelationType.None && definition.Fields.All(y => y.FieldId != x.Id)))
                {
                    article.Fields[plainFieldFromQp.Name] = DeserializeField(new PlainField { FieldId = plainFieldFromQp.Id }, plainFieldFromQp, productDataSource, connector, context);
                }
            }

            return article;
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

        private ArticleField DeserializeExtensionField(ExtensionField fieldInDef, Quantumart.QP8.BLL.Field qpField, IProductDataSource productDataSource, DBConnector connector, Context context)
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
                throw new Exception("В описании BackwardArticleField должен быть непустой FieldName");

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
            else throw new Exception(string.Format("В описании поле id={0} имеет тип EntityField но его RelationType не соответствует требуемым", fieldInDef.FieldId));

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

    internal class Context
    {
        private int _minArticleId;
        private readonly Dictionary<int, List<Article>> _extensionMap;

        public Context()
        {
            _extensionMap = new Dictionary<int, List<Article>>();
            _minArticleId = 0;
        }

        public void AddExtensionArticle(int parentId, Article article)
        {
            if (!_extensionMap.ContainsKey(parentId))
            {
                _extensionMap[parentId] = new List<Article>();
            }

            _extensionMap[parentId].Add(article);
        }

        public void TakeIntoAccount(int id)
        {
            if (id < _minArticleId)
            {
                _minArticleId = id;
            }
        }

        public void UpdateExtensionArticles()
        {
            var id = _minArticleId;
            foreach (var list in _extensionMap.Values)
            {
                id--;

                foreach (var article in list)
                {
                    article.Id = id;
                }
            }
        }
    }
}
