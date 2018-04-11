﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;

namespace QA.Core.DPC.Loader
{
    public class JsonProductService : IJsonProductService
    {
        private const string ID_PROP_NAME = "Id";

        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly ILogger _logger;
        private readonly DBConnector _dbConnector;
        private readonly VirtualFieldPathEvaluator _virtualFieldPathEvaluator;
        private readonly IRegionTagReplaceService _regionTagReplaceService;

        public JsonProductService(
            IConnectionProvider connectionProvider,
            ILogger logger,
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldPathEvaluator virtualFieldPathEvaluator,
            IRegionTagReplaceService regionTagReplaceService)
        {
            _logger = logger;
            _contentService = contentService;
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
        
        private class SchemaContext
        {
            /// <summary>
            /// Набор контентов, которые повторяются при создании схемы
            /// </summary>
            public HashSet<Content> RepeatedContents = new HashSet<Content>();

            /// <summary>
            /// Созданные схемы для различных контентов
            /// </summary>
            public Dictionary<Content, JSchema> SchemasByContent = new Dictionary<Content, JSchema>();

            /// <summary>
            /// Значения вирутальных полей
            /// </summary>
            public Dictionary<Tuple<Content, string>, Field> VirtualFields 
                = new Dictionary<Tuple<Content, string>, Field>();

            /// <summary>
            /// Cписок виртуальных полей которые надо игнорировать при создании схемы
            /// </summary>
            public List<Tuple<Content, Field>> IgnoredFields = new List<Tuple<Content, Field>>();
        }

        public JSchema GetSchema(Content definition, bool forList = false, bool includeRegionTags = false)
        {
            _contentService.LoadStructureCache();
            _fieldService.LoadStructureCache();

            var context = new SchemaContext();

            FillVirtualFieldsInfo(definition, context);

            JSchema rootSchema = GetSchemaRecursive(definition, context, forList, "");

            FillSchemaDefinitions(rootSchema, context);
            
            if (includeRegionTags)
            {
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

        /// <summary>
        /// Выносит повторяющиеся схемы контентов в поле "definitions" объекта <paramref name="rootSchema"/>
        /// </summary>
        private void FillSchemaDefinitions(JSchema rootSchema, SchemaContext context)
        {
            if (context.RepeatedContents.Count == 0)
            {
                return;
            }

            var definitions = new JObject();
            var contentIndexesByName = new Dictionary<string, int>();

            foreach (Content content in context.RepeatedContents)
            {
                JSchema schema = context.SchemasByContent[content];

                string name = schema.ExtensionData.ContainsKey("contentName")
                    ? (string)schema.ExtensionData["contentName"]
                    : null;

                if (String.IsNullOrWhiteSpace(name))
                {
                    name = $"Content{content.ContentId}";
                }

                if (contentIndexesByName.ContainsKey(name))
                {
                    name += contentIndexesByName[name]++;
                }
                else
                {
                    contentIndexesByName[name] = 1;
                }

                definitions[name] = schema;
            }

            rootSchema.ExtensionData["definitions"] = definitions;
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/> и заполняет
        /// <see cref="SchemaContext.IgnoredFields"/> - список полей которые надо игнорировать при создании схемы
        /// и <see cref="SchemaContext.VirtualFields"/> - значения вирутальных полей
        /// </summary>
        private void FillVirtualFieldsInfo(
            Content definition,
            SchemaContext context,
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

            foreach (Field field in definition.Fields)
            {
                if (field is BaseVirtualField baseField)
                {
                    ProcessVirtualField(baseField, definition, context);
                }
                else if (field is EntityField entityField)
                {
                    FillVirtualFieldsInfo(entityField.Content, context, parentContents);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (Content content in extensionField.ContentMapping.Values)
                    {
                        FillVirtualFieldsInfo(content, context, parentContents);
                    }
                }
            }
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/> и заполняет
        /// <see cref="SchemaContext.IgnoredFields"/> - список полей которые надо игнорировать при создании схемы
        /// и <see cref="SchemaContext.VirtualFields"/> - значения вирутальных полей
        /// </summary>
        private void ProcessVirtualField(
            BaseVirtualField baseVirtualField,
            Content definition,
            SchemaContext context)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                string path = virtualMultiEntityField.Path;

                var virtualFieldKey = new Tuple<Content, string>(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                bool hasFilter;
                Content parent;

                Field foundField = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out hasFilter, out parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(new Tuple<Content, Field>(parent, foundField));
                }

                context.VirtualFields[virtualFieldKey] = foundField;

                if (!(foundField is EntityField foundEntityField))
                {
                    throw new Exception("В Path VirtualMultiEntityField должны быть только поля EntityField или наследники");
                }
                foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                {
                    ProcessVirtualField(childField, foundEntityField.Content, context);
                }
            }
            else if (baseVirtualField is VirtualEntityField virtualEntityField)
            {
                foreach (BaseVirtualField childField in virtualEntityField.Fields)
                {
                    ProcessVirtualField(childField, definition, context);
                }
            }
            else if (baseVirtualField is VirtualField virtualField)
            {
                string path = virtualField.Path;

                var virtualFieldKey = new Tuple<Content, string>(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                bool hasFilter;
                Content parent;

                Field fieldToMove = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out hasFilter, out parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(new Tuple<Content, Field>(parent, fieldToMove));
                }

                context.VirtualFields[virtualFieldKey] = fieldToMove;
            }
        }
        
        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/>, генерирует корневую схему и заполняет
        /// <see cref="SchemaContext.SchemasByContent"/> - созданные схемы контентов
        /// и <see cref="SchemaContext.RepeatedContents"/> - повторяющиеся контенты
        /// </summary>
        private JSchema GetSchemaRecursive(Content definition, SchemaContext context, bool forList, string path)
        {
            path += $"/{definition.ContentId}";

            if (context.SchemasByContent.ContainsKey(definition))
            {
                context.RepeatedContents.Add(definition);
                return context.SchemasByContent[definition];
            }
            
            var contentSchema = CreateContentSchema(definition, path);

            context.SchemasByContent[definition] = contentSchema;

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
                        baseVirtualField, definition, context);
                }
                else if (!context.IgnoredFields.Contains(new Tuple<Content, Field>(definition, field)))
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
                        MergeExtensionFieldSchema(extensionField, qpField, contentSchema, context, path);
                    }
                    else
                    {
                        contentSchema.Properties[field.FieldName] = GetFieldSchema(
                            field, qpField, context, forList, path);
                    }
                }
            }

            if (definition.LoadAllPlainFields && !forList)
            {
                var fieldsToAdd = qpFields
                    .Where(f => f.RelationType == Quantumart.QP8.BLL.RelationType.None
                        && definition.Fields.All(y => y.FieldId != f.Id))
                    .Where(f => !context.IgnoredFields
                        .Any(t => t.Item1.Equals(definition) && t.Item2.FieldId == f.Id));

                foreach (var qpField in fieldsToAdd)
                {
                    contentSchema.Properties[qpField.Name] = GetPlainFieldSchema(qpField);
                }
            }

            if (forList)
            {
                return new JSchema { Type = JSchemaType.Array, Items = { contentSchema } };
            }

            return contentSchema;
        }

        private JSchema CreateContentSchema(Content content, string path)
        {
            var qpContent = _contentService.Read(content.ContentId);

            var schema = new JSchema { Type = JSchemaType.Object, AllowAdditionalProperties = false };

            schema.Description =
                !IsHtmlWhiteSpace(qpContent.Description)
                    ? qpContent.Description
                    : !IsHtmlWhiteSpace(qpContent.FriendlySingularName)
                        ? qpContent.FriendlySingularName
                        : !IsHtmlWhiteSpace(qpContent.Name)
                            ? qpContent.Name
                            : null;

            schema.ExtensionData["contentId"] = qpContent.Id;
            schema.ExtensionData["contentPath"] = path;

            if (!String.IsNullOrWhiteSpace(qpContent.NetName))
            {
                schema.ExtensionData["contentName"] = qpContent.NetName;
            }
            if (!IsHtmlWhiteSpace(qpContent.Name))
            {
                schema.ExtensionData["contentTitle"] = qpContent.Name;
            }
            if (!IsHtmlWhiteSpace(qpContent.Description))
            {
                schema.ExtensionData["contentDescription"] = qpContent.Description;
            }

            return schema;
        }

        /// <param name="field">Can be null</param>
        private static JSchema AttachFieldData(Field field, Quantumart.QP8.BLL.Field qpField, JSchema schema)
        {
            schema.Description = 
                !IsHtmlWhiteSpace(qpField.Description)
                    ? qpField.Description
                    : !IsHtmlWhiteSpace(qpField.FriendlyName)
                        ? qpField.FriendlyName
                        : schema.Description;

            // TODO: schema.Default

            schema.ExtensionData["fieldId"] = field?.FieldId ?? qpField.Id;
            schema.ExtensionData["fieldName"] = field?.FieldName ?? qpField.Name;

            string fieldType = field?.FieldType ?? qpField.ExactType.ToString();

            if (!String.IsNullOrEmpty(fieldType))
            {
                schema.ExtensionData["fieldType"] = fieldType;
            }
            if (!IsHtmlWhiteSpace(qpField.FriendlyName))
            {
                schema.ExtensionData["fieldTitle"] = qpField.FriendlyName;
            }
            if (!IsHtmlWhiteSpace(qpField.Description))
            {
                schema.ExtensionData["fieldDescription"] = qpField.Description;
            }
            
            return schema;
        }

        private static bool IsHtmlWhiteSpace(string str)
        {
            return String.IsNullOrEmpty(str) 
                || String.IsNullOrWhiteSpace(WebUtility.HtmlDecode(str));
        }

        private void MergeExtensionFieldSchema(
            ExtensionField field,
            Quantumart.QP8.BLL.Field qpField,
            JSchema contentSchema,
            SchemaContext context,
            string path)
        {
            path += $":{field.FieldId}";

            JSchema extensionSchema = AttachFieldData(field, qpField, new JSchema { Type = JSchemaType.String });

            contentSchema.Properties.Add(field.FieldName, extensionSchema);

            var contentFieldGroups = field.ContentMapping.Values
                .SelectMany(c => c.Fields
                    .Select(f => new { Content = c, Field = f }))
                .GroupBy(x => x.Field.FieldName);

            foreach (var contentFieldGroup in contentFieldGroups)
            {
                JSchema[] groupSchemas = contentFieldGroup
                    .Select(contentField =>
                    {
                        Content extContent = contentField.Content;
                        Field extField = contentField.Field;

                        var extQpField = _fieldService.Read(extField.FieldId);

                        string extPath = $"{path}/{extContent.ContentId}";

                        return GetFieldSchema(extField, extQpField, context, false, extPath);
                    })
                    .ToArray();

                JSchema sameNameExtensionFieldsSchema;
                if (groupSchemas.Length > 1)
                {
                    sameNameExtensionFieldsSchema = new JSchema { Type = JSchemaType.Object };

                    foreach (JSchema extFieldSchema in groupSchemas)
                    {
                        sameNameExtensionFieldsSchema.OneOf.Add(extFieldSchema);
                    }
                }
                else
                {
                    sameNameExtensionFieldsSchema = groupSchemas[0];
                }

                contentSchema.Properties[contentFieldGroup.Key] = sameNameExtensionFieldsSchema;
            }
        }

        /// <param name="qpField">Can be null</param>
        private JSchema GetFieldSchema(
            Field field, Quantumart.QP8.BLL.Field qpField, SchemaContext context, bool forList, string path)
        {
            path += $":{field.FieldId}";

            if (qpField == null && !(field is BaseVirtualField))
            {
                qpField = _fieldService.Read(field.FieldId);
            }

            if (field is PlainField plainField)
            {
                return GetPlainFieldSchema(qpField, plainField);
            }
            else if (field is BackwardRelationField backwardRelationalField)
            {
                JSchema backwardItemSchema = GetSchemaRecursive(backwardRelationalField.Content, context, forList, path);

                return AttachFieldData(field, qpField, new JSchema
                {
                    Type = JSchemaType.Array, Items = { backwardItemSchema }
                });
            }
            else if (field is EntityField entityField)
            {
                Content fieldContent = entityField.Content;

                if (qpField.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany)
                {
                    return AttachFieldData(field, qpField, GetSchemaRecursive(fieldContent, context, forList, path));
                }
                else
                {
                    JSchema arrayItemSchema = GetSchemaRecursive(fieldContent, context, forList, path);

                    return AttachFieldData(field, qpField, new JSchema
                    {
                        Type = JSchemaType.Array, Items = { arrayItemSchema }
                    });
                }
            }
            else
            {
                throw new Exception($"Поля типа {field.GetType()} не поддерживается");
            }
        }
        
        private JSchema GetVirtualFieldSchema(
            BaseVirtualField baseVirtualField, Content definition, SchemaContext context)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                var boundFieldKey = new Tuple<Content, string>(definition, virtualMultiEntityField.Path);

                Field boundField = context.VirtualFields[boundFieldKey];

                var contentForArrayFields = ((EntityField)boundField).Content;

                var itemSchema = new JSchema { Type = JSchemaType.Object };

                foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                {
                    JSchema childfieldSchema = GetVirtualFieldSchema(childField, contentForArrayFields, context);

                    itemSchema.Properties[childField.FieldName] = childfieldSchema;
                }

                return new JSchema { Type = JSchemaType.Array, Items = { itemSchema } };
            }
            else if (baseVirtualField is VirtualEntityField virtualEntityField)
            {
                BaseVirtualField[] fields = virtualEntityField.Fields;

                var virtualEntityFieldSchema = new JSchema { Type = JSchemaType.Object };

                foreach (BaseVirtualField childField in fields)
                {
                    JSchema childfieldSchema = GetVirtualFieldSchema(childField, definition, context);

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

                    return GetFieldSchema(context.VirtualFields[virtualFieldKey], null, context, false, "/virtual");
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
        
        private static JSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field qpField, PlainField field = null)
        {
            var schema = new JSchema();

            switch (qpField.ExactType)
            {
                case FieldExactTypes.Numeric:
                    schema.Type = qpField.IsInteger ? JSchemaType.Integer : JSchemaType.Number;
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

            return AttachFieldData(field, qpField, schema);
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

        internal class PlainFieldFileInfo
        {
            public string Name { get; set; }

            public int FileSizeBytes { get; set; }

            public string AbsoluteUrl { get; set; }
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
