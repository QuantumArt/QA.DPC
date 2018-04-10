using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;
using Article = QA.Core.Models.Entities.Article;
using Content = QA.Core.Models.Configuration.Content;
using Field = QA.Core.Models.Configuration.Field;
using FieldService = Quantumart.QP8.BLL.Services.API.FieldService;

namespace QA.Core.DPC.Loader
{
    public class JsonProductService : IJsonProductService
    {
        private const string ID_PROP_NAME = "Id";

        private readonly FieldService _fieldService;
        private readonly ILogger _logger;
        private readonly DBConnector _dbConnector;
        private readonly VirtualFieldPathEvaluator _virtualFieldPathEvaluator;
        private readonly IRegionTagReplaceService _regionTagReplaceService;

        internal class PlainFieldFileInfo
        {
            public string Name { get; set; }

            public int FileSizeBytes { get; set; }

            public string AbsoluteUrl { get; set; }
        }

        public JsonProductService(
            IConnectionProvider connectionProvider,
            ILogger logger,
            FieldService fieldService,
            VirtualFieldPathEvaluator virtualFieldPathEvaluator,
            IRegionTagReplaceService regionTagReplaceService)
        {
            _logger = logger;
            _fieldService = fieldService;

            var connectionString = connectionProvider.GetConnection();
            _dbConnector = new DBConnector(connectionString);

            _virtualFieldPathEvaluator = virtualFieldPathEvaluator;
            _regionTagReplaceService = regionTagReplaceService;
        }

        public Article DeserializeProduct(string productJson, Content definition)
        {
            var rootArticleDictionary = JsonConvert.DeserializeObject<JObject>(productJson);

            var product = rootArticleDictionary.SelectToken("product");
            if (product != null)
            {
                rootArticleDictionary = (JObject)product;
            }

            var productDeserializer = ObjectFactoryBase.Resolve<IProductDeserializer>();

            return productDeserializer.Deserialize(new JsonProductDataSource(rootArticleDictionary), definition);
        }

        public string SerializeProduct(Article article, IArticleFilter filter, bool includeRegionTags = false)
        {
            string productJson = JsonConvert.SerializeObject(ConvertArticle(article, filter), Formatting.Indented);
            var regionField = article.GetField("Regions") as MultiArticleField;

            if (includeRegionTags && regionField != null)
            {
                int[] regionIds = regionField.Items.Keys.ToArray();

                TagWithValues[] tags = _regionTagReplaceService.GetRegionTagValues(productJson, regionIds);

                string tagsJson = JsonConvert.SerializeObject(
                    tags,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                return $"{{ \"{_productPropertyName}\" : {productJson}, \"{_regionTagsPropertyName}\" : {tagsJson} }}";
            }

            return $"{{ \"{_productPropertyName}\" : {productJson} }}";
        }

        private const string _productPropertyName = "product";
        private const string _regionTagsPropertyName = "regionTags";
        
        public JSchema GetSchema(Content definition, bool forList = false, bool includeRegionTags = false)
        {
            var foundFields = new Dictionary<Tuple<Content, string>, Field>();

            var fieldsToIgnore = new List<Tuple<Content, Field>>();

            FillVirtualFieldsInfo(definition, foundFields, fieldsToIgnore);

            JSchema rootSchema = GetSchemaRecursive(
                definition, forList, new Dictionary<Content, JSchema>(), foundFields, fieldsToIgnore);

            if (includeRegionTags)
            {
                // todo
                JSchema schemaWithRegionTags = new JSchema { Type = JSchemaType.Object };

                schemaWithRegionTags.Properties.Add(_productPropertyName, rootSchema);

                JSchema regionTagsSchema = new JSchema { Type = JSchemaType.Array };

                JSchema regionTagSchema = new JSchema { Type = JSchemaType.Object };

                regionTagSchema.Properties.Add("title", new JSchema { Type = JSchemaType.String });

                JSchema valuesSchema = new JSchema { Type = JSchemaType.Array };

                JSchema valueSchema = new JSchema { Type = JSchemaType.Object };

                valueSchema.Properties.Add("value", new JSchema { Type = JSchemaType.String });

                JSchema regionIdsSchema = new JSchema { Type = JSchemaType.Array };

                regionIdsSchema.Items.Add(new JSchema { Type = JSchemaType.Integer });

                valueSchema.Properties.Add("regionsId", regionIdsSchema);

                valueSchema.Required.Add("regionsId");

                valueSchema.Required.Add("value");

                valuesSchema.Items.Add(valueSchema);

                regionTagSchema.Properties.Add("values", valuesSchema);

                regionTagSchema.Required.Add("title");

                regionTagSchema.Required.Add("values");

                regionTagsSchema.Items.Add(regionTagSchema);

                schemaWithRegionTags.Properties.Add(_regionTagsPropertyName, regionTagsSchema);

                return schemaWithRegionTags;
            }

            return rootSchema;
        }

        private void FillVirtualFieldsInfo(
            Content definition,
            Dictionary<Tuple<Content, string>, Field> foundFields,
            List<Tuple<Content, Field>> fieldsToDelete,
            HashSet<Content> parentContents = null)
        {
            if (parentContents == null)
            {
                parentContents = new HashSet<Content>();
            }
            else if (parentContents.Contains(definition))
            {
                return;
            }

            parentContents.Add(definition);

            foreach (var field in definition.Fields)
            {
                if (field is BaseVirtualField baseField)
                {
                    ProcessVirtualField(baseField, definition, foundFields, fieldsToDelete);
                }
                else if (field is EntityField entityField)
                {
                    FillVirtualFieldsInfo(entityField.Content, foundFields, fieldsToDelete, parentContents);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (Content content in extensionField.ContentMapping.Values)
                    {
                        FillVirtualFieldsInfo(content, foundFields, fieldsToDelete, parentContents);
                    }
                }
            }
        }

        /// <summary>
        /// Рекурсивно обходит Content и заполняет список полей которые
        /// надо игнорировать при создании схемы и значения вирутальных полей
        /// </summary>
        private void ProcessVirtualField(
            BaseVirtualField baseVirtualField,
            Content definition,
            Dictionary<Tuple<Content, string>, Field> foundFields,
            List<Tuple<Content, Field>> fieldsToDelete)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                string path = virtualMultiEntityField.Path;

                var keyInFoundFieldsDic = new Tuple<Content, string>(definition, path);

                if (foundFields.ContainsKey(keyInFoundFieldsDic))
                {
                    return;
                }

                bool hasFilter;
                Content parent;

                Field foundField = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out hasFilter, out parent);

                if (!hasFilter)
                {
                    fieldsToDelete.Add(new Tuple<Content, Field>(parent, foundField));
                }

                foundFields[keyInFoundFieldsDic] = foundField;

                if (!(foundField is EntityField foundEntityField))
                {
                    throw new Exception("В Path VirtualMultiEntityField должны быть только поля EntityField или наследники");
                }
                foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                {
                    ProcessVirtualField(childField, foundEntityField.Content, foundFields, fieldsToDelete);
                }
            }
            else if (baseVirtualField is VirtualEntityField virtualEntityField)
            {
                foreach (BaseVirtualField childField in virtualEntityField.Fields)
                {
                    ProcessVirtualField(childField, definition, foundFields, fieldsToDelete);
                }
            }
            else if (baseVirtualField is VirtualField virtualField)
            {
                string path = virtualField.Path;

                var keyInFoundFieldsDic = new Tuple<Content, string>(definition, path);

                if (foundFields.ContainsKey(keyInFoundFieldsDic))
                {
                    return;
                }

                bool hasFilter;
                Content parent;

                Field fieldToMove = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out hasFilter, out parent);

                if (!hasFilter)
                {
                    fieldsToDelete.Add(new Tuple<Content, Field>(parent, fieldToMove));
                }

                foundFields[keyInFoundFieldsDic] = fieldToMove;
            }
        }
        
        private JSchema GetSchemaRecursive(
            Content definition,
            bool forList,
            Dictionary<Content, JSchema> generatedSchemas,
            Dictionary<Tuple<Content, string>, Field> foundVirtualFields,
            List<Tuple<Content, Field>> fieldsToIgnore)
        {
            if (generatedSchemas.ContainsKey(definition))
            {
                return generatedSchemas[definition];
            }

            var contentSchema = new JSchema { Type = JSchemaType.Object };

            generatedSchemas[definition] = contentSchema;

            contentSchema.Properties.Add(ID_PROP_NAME, new JSchema
            {
                Type = JSchemaType.Integer, Minimum = 0, ExclusiveMinimum = true
            });

            contentSchema.Required.Add(ID_PROP_NAME);

            var qpFields = _fieldService.List(definition.ContentId).ToArray();

            foreach (Field field in definition.Fields
                .Where(f => !(f is Dictionaries)
                    && (!forList || f is PlainField plainField && plainField.ShowInList)))
            {
                if (field.FieldName == null)
                {
                    throw new InvalidOperationException(
                        $"FieldName is null: {new { field.FieldId, field.FieldName }}");
                }

                if (field is BaseVirtualField baseVirtualField)
                {
                    contentSchema.Properties[field.FieldName] = GetVirtualFieldSchema(
                        baseVirtualField, definition, generatedSchemas, fieldsToIgnore, foundVirtualFields);
                }
                else if (!fieldsToIgnore.Contains(new Tuple<Content, Field>(definition, field)))
                {
                    var qpField = field is BackwardRelationField
                        ? _fieldService.Read(field.FieldId)
                        : qpFields.SingleOrDefault(x => x.Id == field.FieldId);

                    if (qpField == null)
                    {
                        throw new Exception($@"В definition указано поле id={
                            field.FieldId} которого нет в контенте id={definition.ContentId}");
                    }
                    if (field is ExtensionField extensionField)
                    {
                        MergeExtensionFieldSchema(
                            extensionField, contentSchema, generatedSchemas, foundVirtualFields, fieldsToIgnore);
                    }
                    else
                    {
                        contentSchema.Properties[field.FieldName] = GetFieldSchema(
                            field, qpField, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore);
                    }
                }
            }

            if (definition.LoadAllPlainFields && !forList)
            {
                var fieldsToAdd = qpFields
                    .Where(f => f.RelationType == Quantumart.QP8.BLL.RelationType.None
                        && definition.Fields.All(y => y.FieldId != f.Id))
                    .Where(f => !fieldsToIgnore.Any(t => t.Item1.Equals(definition) && t.Item2.FieldId == f.Id));

                foreach (var qpField in fieldsToAdd)
                {
                    contentSchema.Properties[qpField.Name] = GetPlainFieldSchema(qpField);
                }
            }

            if (forList)
            {
                var arraySchema = new JSchema { Type = JSchemaType.Array };

                arraySchema.Items.Add(contentSchema);

                return arraySchema;
            }

            return contentSchema;
        }

        private void MergeExtensionFieldSchema(
            ExtensionField field,
            JSchema contentSchema,
            Dictionary<Content, JSchema> generatedSchemas,
            Dictionary<Tuple<Content, string>, Field> foundVirtualFields,
            List<Tuple<Content, Field>> fieldsToIgnore)
        {
            contentSchema.Properties.Add(field.FieldName, new JSchema { Type = JSchemaType.String });

            var contentFieldGroups = field.ContentMapping.Values
                .SelectMany(x => x.Fields)
                .GroupBy(x => x.FieldName);

            foreach (var contentFieldGroup in contentFieldGroups)
            {
                Field[] groupFields = contentFieldGroup.ToArray();

                JSchema sameNameExtensionFieldsSchema;
                if (groupFields.Length > 1)
                {
                    sameNameExtensionFieldsSchema = new JSchema { Type = JSchemaType.Object };

                    foreach (Field extField in groupFields)
                    {
                        JSchema extFieldSchema = GetFieldSchema(
                            extField, _fieldService.Read(extField.FieldId), false,
                            generatedSchemas, foundVirtualFields, fieldsToIgnore);

                        sameNameExtensionFieldsSchema.OneOf.Add(extFieldSchema);
                    }
                }
                else
                {
                    sameNameExtensionFieldsSchema = GetFieldSchema(
                        groupFields[0], _fieldService.Read(groupFields[0].FieldId), false,
                        generatedSchemas, foundVirtualFields, fieldsToIgnore);
                }

                contentSchema.Properties[contentFieldGroup.Key] = sameNameExtensionFieldsSchema;
            }
        }

        private JSchema GetFieldSchema(
            Field field,
            Quantumart.QP8.BLL.Field qpField,
            bool forList, Dictionary<Content, JSchema> generatedSchemas,
            Dictionary<Tuple<Content, string>, Field> foundVirtualFields,
            List<Tuple<Content, Field>> fieldsToIgnore)
        {
            if (qpField == null && !(field is BaseVirtualField))
            {
                qpField = _fieldService.Read(field.FieldId);
            }

            if (field is PlainField)
            {
                return GetPlainFieldSchema(qpField);
            }
            else if (field is BackwardRelationField backwardRelationalField)
            {
                var backwardFieldSchema = new JSchema { Type = JSchemaType.Array };

                JSchema backwardItemSchema = GetSchemaRecursive(
                    backwardRelationalField.Content, forList, generatedSchemas,
                    foundVirtualFields, fieldsToIgnore);

                backwardFieldSchema.Items.Add(backwardItemSchema);

                return backwardFieldSchema;
            }
            else if (field is EntityField entityField)
            {
                Content fieldContent = entityField.Content;

                if (qpField.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany)
                {
                    return GetSchemaRecursive(
                        fieldContent, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore);
                }
                else
                {
                    var arrayFieldSchema = new JSchema { Type = JSchemaType.Array };

                    JSchema arrayItemSchema = GetSchemaRecursive(
                        fieldContent, forList, generatedSchemas, foundVirtualFields, fieldsToIgnore);

                    arrayFieldSchema.Items.Add(arrayItemSchema);

                    return arrayFieldSchema;
                }
            }
            else
            {
                throw new Exception($"Поля типа {field.GetType()} не поддерживается");
            }
        }
        
        private JSchema GetVirtualFieldSchema(
            BaseVirtualField baseVirtualField,
            Content definition,
            Dictionary<Content, JSchema> generatedSchemas,
            List<Tuple<Content, Field>> fieldsToIgnore,
            Dictionary<Tuple<Content, string>, Field> foundVirtualFields)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                var boundFieldKey = new Tuple<Content, string>(definition, virtualMultiEntityField.Path);

                Field boundField = foundVirtualFields[boundFieldKey];

                var contentForArrayFields = ((EntityField)boundField).Content;

                var itemSchema = new JSchema { Type = JSchemaType.Object };

                foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                {
                    JSchema childfieldSchema = GetVirtualFieldSchema(
                        childField, contentForArrayFields, generatedSchemas, fieldsToIgnore, foundVirtualFields);

                    itemSchema.Properties[childField.FieldName] = childfieldSchema;
                }

                var virtualMultiEntityFieldSchema = new JSchema { Type = JSchemaType.Array };

                virtualMultiEntityFieldSchema.Items.Add(itemSchema);

                return virtualMultiEntityFieldSchema;
            }
            else if (baseVirtualField is VirtualEntityField virtualEntityField)
            {
                BaseVirtualField[] fields = virtualEntityField.Fields;

                var virtualEntityFieldSchema = new JSchema { Type = JSchemaType.Object };

                foreach (BaseVirtualField childField in fields)
                {
                    JSchema childfieldSchema = GetVirtualFieldSchema(
                        childField, definition, generatedSchemas, fieldsToIgnore, foundVirtualFields);

                    virtualEntityFieldSchema.Properties[childField.FieldName] = childfieldSchema;
                }

                return virtualEntityFieldSchema;
            }
            else if (baseVirtualField is VirtualField virtualField)
            {
                if (virtualField.Converter != null)
                {
                    return new JSchema { Type = ConvertTypeToJsType(virtualField.Converter.OutputType) };
                }
                else
                {
                    var virtualFieldKey = new Tuple<Content, string>(definition, virtualField.Path);

                    return GetFieldSchema(
                        foundVirtualFields[virtualFieldKey], null, false,
                        generatedSchemas, foundVirtualFields, fieldsToIgnore);
                }
            }
            else
            {
                throw new Exception($"Поле типа {baseVirtualField.GetType()} не поддерживается");
            }
        }

        private static JSchemaType ConvertTypeToJsType(Type type)
        {
            if (type == typeof(bool))
                return JSchemaType.Boolean;

            if (type == typeof(string))
                return JSchemaType.String;

            if (type == typeof(int))
                return JSchemaType.Integer;

            throw new Exception("Схема не поддерживает конвертер возвращающий тип " + type);
        }
        
        private static JSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field field)
        {
            var schema = new JSchema();

            switch (field.ExactType)
            {
                case FieldExactTypes.Numeric:
                    schema.Type = field.IsInteger ? JSchemaType.Integer : JSchemaType.Number;
                    break;

                case FieldExactTypes.File:
                    schema.Type = JSchemaType.Object;

                    schema.Required.Add("Name");
                    schema.Required.Add("AbsoluteUrl");
                    schema.Required.Add("FileSizeBytes");

                    schema.Properties["Name"] = new JSchema { Type = JSchemaType.String };

                    schema.Properties["AbsoluteUrl"] = new JSchema { Type = JSchemaType.String };

                    schema.Properties["FileSizeBytes"] = new JSchema
                    {
                        Type = JSchemaType.Integer,
                        Minimum = 0,
                        ExclusiveMinimum = false
                    };
                    break;

                case FieldExactTypes.Boolean:
                    schema.Type = JSchemaType.Boolean;
                    break;

                case FieldExactTypes.O2MRelation:
                    schema.Type = JSchemaType.Integer;
                    break;

                default:
                    schema.Type = JSchemaType.String;
                    break;
            }

            return schema;
        }

        private object GetPlainArticleFieldValue(PlainArticleField plainArticleField)
        {
            if (plainArticleField.NativeValue == null)
            {
                return null;
            }

            switch (plainArticleField.PlainFieldType)
            {
                case PlainFieldType.File:
                {
                    if (string.IsNullOrWhiteSpace(plainArticleField.Value))
                    {
                        return null;
                    }

                    string path = Common.GetFileFromQpFieldPath(
                        _dbConnector, plainArticleField.FieldId.Value, plainArticleField.Value);

                    int size = 0;
                    try
                    {
                        var fi = new FileInfo(path);
                        size = (int)fi.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("DBConnector error", ex);
                    }

                    return new PlainFieldFileInfo
                    {
                        Name = plainArticleField.Value.Contains("/")
                            ? plainArticleField.Value.Substring(plainArticleField.Value.LastIndexOf("/") + 1)
                            : plainArticleField.Value,

                        FileSizeBytes = size,

                        AbsoluteUrl = string.Format("{0}/{1}",
                            _dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
                            plainArticleField.Value)
                    };
                }

                case PlainFieldType.Image:
                case PlainFieldType.DynamicImage:
                {
                    if (string.IsNullOrWhiteSpace(plainArticleField.Value))
                    {
                        return null;
                    }

                    return string.Format(@"{0}/{1}",
                        _dbConnector.GetUrlForFileAttribute(plainArticleField.FieldId.Value, true, true),
                        plainArticleField.Value);
                }

                case PlainFieldType.Boolean:
                    return (decimal)plainArticleField.NativeValue == 1;

                default:
                    return plainArticleField.NativeValue;
            }
        }

        private Dictionary<string, object> ConvertArticle(Article article, IArticleFilter filter)
        {
            if (article == null || !article.Visible || article.Archived || !filter.Matches(article))
            {
                return null;
            }

            var dict = new Dictionary<string, object> { { ID_PROP_NAME, article.Id } };

            foreach (ArticleField field in article.Fields.Values)
            {
                if (field is ExtensionArticleField extensionArticleField)
                {
                    MergeExtensionFields(dict, extensionArticleField, filter);
                }
                else
                {
                    object value = ConvertField(field, filter);

                    if (value != null && !(value is string && string.IsNullOrEmpty((string)value)))
                    {
                        dict[field.FieldName] = value;
                    }
                }
            }

            return dict;
        }

        private void MergeExtensionFields(
            Dictionary<string, object> dic, ExtensionArticleField field, IArticleFilter filter)
        {
            if (field.Item == null)
            {
                return;
            }

            dic[field.FieldName] = field.Item.ContentName;

            foreach (ArticleField childField in field.Item.Fields.Values)
            {
                object value = ConvertField(childField, filter);

                if (value != null && !(value is string && string.IsNullOrEmpty((string)value)))
                {
                    dic[childField.FieldName] = value;
                }
            }
        }

        private object ConvertField(ArticleField field, IArticleFilter filter)
        {
            if (field is PlainArticleField plainArticleField)
            {
                return GetPlainArticleFieldValue(plainArticleField);
            }
            if (field is SingleArticleField singleArticleField)
            {
                return ConvertArticle(singleArticleField.GetItem(filter), filter);
            }
            if (field is MultiArticleField multiArticleField)
            {
                var articles = multiArticleField
                    .GetArticles(filter)
                    .Select(x => ConvertArticle(x, filter))
                    .ToArray();

                return articles.Length == 0 ? null : articles;
            }
            if (field is VirtualArticleField virtualArticleField)
            {
                return virtualArticleField.Fields
                    .Select(x => new { fieldName = x.FieldName, value = ConvertField(x, filter) })
                    .ToDictionary(x => x.fieldName, x => x.value);
            }
            if (field is VirtualMultiArticleField virtualMultiArticleField)
            {
                return virtualMultiArticleField.VirtualArticles.Select(x => ConvertField(x, filter));
            }

            throw new Exception($"Поле типа {field.GetType()} не поддерживается");
        }
    }
}
