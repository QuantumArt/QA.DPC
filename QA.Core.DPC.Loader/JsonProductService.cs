using System;
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
        private const string IdProp = "Id";
        private const string ProductProp = "product";
        private const string RegionTagsProp = "regionTags";

        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly ILogger _logger;
        private readonly DBConnector _dbConnector;
        private readonly VirtualFieldContextService _virtualFieldContextService;
        private readonly IRegionTagReplaceService _regionTagReplaceService;

        public JsonProductService(
            IConnectionProvider connectionProvider,
            ILogger logger,
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService,
            IRegionTagReplaceService regionTagReplaceService)
        {
            _logger = logger;
            _contentService = contentService;
            _fieldService = fieldService;

            var connectionString = connectionProvider.GetConnection();
            _dbConnector = new DBConnector(connectionString);

            _virtualFieldContextService = virtualFieldContextService;
            _regionTagReplaceService = regionTagReplaceService;
        }

        public Article DeserializeProduct(string productJson, Content definition)
        {
            return DeserializeProduct(JsonConvert.DeserializeObject<JObject>(productJson), definition);
        }

        public Article DeserializeProduct(JObject rootArticleDictionary, Content definition)
        {
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
            Dictionary<string, object> product = ConvertArticle(article, filter);

            string productJson = JsonConvert.SerializeObject(product, Formatting.Indented);
            
            if (includeRegionTags && article.GetField("Regions") is MultiArticleField regionField)
            {
                int[] regionIds = regionField.Items.Keys.ToArray();

                TagWithValues[] tags = _regionTagReplaceService.GetRegionTagValues(productJson, regionIds);

                string tagsJson = JsonConvert.SerializeObject(
                    tags,
                    Formatting.Indented,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                return $"{{ \"{ProductProp}\" : {productJson}, \"{RegionTagsProp}\" : {tagsJson} }}";
            }

            return $"{{ \"{ProductProp}\" : {productJson} }}";
        }
        
        private class SchemaContext
        {
            /// <summary>
            /// Исключать ли из схемы виртуальные поля
            /// </summary>
            public bool ExcludeVirtualFields;

            /// <summary>
            /// Значения вирутальных полей
            /// </summary>
            public Dictionary<Tuple<Content, string>, Field> VirtualFields;

            /// <summary>
            /// Cписок виртуальных полей которые надо игнорировать при создании схемы
            /// </summary>
            public List<Tuple<Content, Field>> IgnoredFields;

            /// <summary>
            /// Набор контентов, которые повторяются при создании схемы
            /// </summary>
            public HashSet<Content> RepeatedContents = new HashSet<Content>();

            /// <summary>
            /// Созданные схемы для различных контентов
            /// </summary>
            public Dictionary<Content, JSchema> SchemasByContent = new Dictionary<Content, JSchema>();

            /// <summary>
            /// Имена ссылок на повторяющиеся схемы контентов
            /// </summary>
            public Dictionary<JSchema, string> DefinitionNamesBySchema = new Dictionary<JSchema, string>();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public string GetEditorJsonSchemaString(Content definition)
        {
            JSchema schema = GetSchema(
                definition,
                forList: false,
                includeRegionTags: false,
                excludeVirtualFields: true);
            
            return JsonConvert.SerializeObject(schema);
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public JSchema GetSchema(Content definition, bool forList = false, bool includeRegionTags = false)
        {
            return GetSchema(definition, forList, includeRegionTags, excludeVirtualFields: false);
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private JSchema GetSchema(Content definition, bool forList, bool includeRegionTags, bool excludeVirtualFields)
        {
            _contentService.LoadStructureCache();
            _fieldService.LoadStructureCache();

            VirtualFieldContext virtualFieldContext = _virtualFieldContextService.GetVirtualFieldContext(definition);

            var context = new SchemaContext
            {
                VirtualFields = virtualFieldContext.VirtualFields,
                IgnoredFields = virtualFieldContext.IgnoredFields,
                ExcludeVirtualFields = excludeVirtualFields
            };

            JSchema rootSchema = GetSchemaRecursive(definition, context, forList);
            
            if (includeRegionTags)
            {
                JSchema schemaWithRegionTags = new JSchema { Type = JSchemaType.Object };

                schemaWithRegionTags.Properties.Add(ProductProp, rootSchema);

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

                schemaWithRegionTags.Properties.Add(RegionTagsProp, regionTagsSchema);

                FillSchemaDefinitions(schemaWithRegionTags, context);

                DeduplicateJsonSchema(schemaWithRegionTags, context);

                return schemaWithRegionTags;
            }
            
            FillSchemaDefinitions(rootSchema, context);

            DeduplicateJsonSchema(rootSchema, context);

            return rootSchema;
        }

        /// <summary>
        /// Выносит повторяющиеся схемы контентов в поле "definitions" объекта <paramref name="rootSchema"/>
        /// и заполняет ими <see cref="SchemaContext.DefinitionNamesBySchema"/>
        /// </summary>
        private void FillSchemaDefinitions(JSchema rootSchema, SchemaContext context)
        {
            if (context.RepeatedContents.Count == 0)
            {
                return;
            }

            var contentNamePairs = context.RepeatedContents
                .GroupBy(content =>
                {
                    string name = _contentService.Read(content.ContentId).NetName;

                    return String.IsNullOrWhiteSpace(name) ? $"Content{content.ContentId}" : name;
                })
                .SelectMany(group => group.Select((content, i) => new
                {
                    Content = content,
                    SchemaName = $"{group.Key}{(i == 0 ? "" : i.ToString())}",
                }));

            var definitions = new JObject();

            foreach (var pair in contentNamePairs)
            {
                JSchema schema = context.SchemasByContent[pair.Content];

                context.DefinitionNamesBySchema[schema] = pair.SchemaName;

                definitions[pair.SchemaName] = schema;
            }

            rootSchema.ExtensionData["definitions"] = definitions;
        }
        
        /// <summary>
        /// Заменяет вхождения повторяющихся JSON схем на JSON Pointer вида
        /// <c>{ "$ref": "#/definitions/ContentName" }</c>.
        /// </summary>
        /// <remarks>
        /// <see cref="JSchema"/> не до конца совместима с RFC 6901: JSON Pointer.
        /// По-умолчанию генерируются ссылки вида
        /// <c>{ "$ref": "#/definitions/MarketingProduct/properties/MarketingDevices/items/0" }</c>
        /// Поэтому используется кастомная генерация ссылок.
        /// </remarks>
        private void DeduplicateJsonSchema(JSchema rootSchema, SchemaContext context)
        {
            foreach (JSchema definitionSchema in context.DefinitionNamesBySchema.Keys)
            {
                DeduplicateJsonSchemaFields(definitionSchema, context);
            }

            DeduplicateJsonSchemaFields(rootSchema, context);
        }
        
        private void DeduplicateJsonSchemaFields(JSchema schema, SchemaContext context)
        {
            schema.AdditionalProperties = DeduplicateJsonSchemaInstance(schema.AdditionalProperties, context);
            schema.AdditionalItems = DeduplicateJsonSchemaInstance(schema.AdditionalItems, context);
            schema.Not = DeduplicateJsonSchemaInstance(schema.Not, context);

            DeduplicateJsonSchemaList(schema.OneOf, context);
            DeduplicateJsonSchemaList(schema.AllOf, context);
            DeduplicateJsonSchemaList(schema.AnyOf, context);
            DeduplicateJsonSchemaList(schema.Items, context);

            DeduplicateJsonSchemaDict(schema.PatternProperties, context);
            DeduplicateJsonSchemaDict(schema.Properties, context);
        }

        private void DeduplicateJsonSchemaList(IList<JSchema> schemaList, SchemaContext context)
        {
            if (schemaList?.Count > 0)
            {
                JSchema[] schemaListItems = schemaList.ToArray();

                schemaList.Clear();

                foreach (JSchema schema in schemaListItems)
                {
                    schemaList.Add(DeduplicateJsonSchemaInstance(schema, context));
                }
            }
        }

        private void DeduplicateJsonSchemaDict(IDictionary<string , JSchema> schemaDict, SchemaContext context)
        {
            if (schemaDict?.Count > 0)
            {
                var schemaDictPairs = schemaDict.ToArray();

                schemaDict.Clear();

                foreach (var pair in schemaDictPairs)
                {
                    schemaDict[pair.Key] = DeduplicateJsonSchemaInstance(pair.Value, context);
                }
            }
        }
        
        private JSchema DeduplicateJsonSchemaInstance(JSchema schema, SchemaContext context)
        {
            if (schema == null || schema.ExtensionData.ContainsKey("$ref"))
            {
                return schema;
            }

            if (context.DefinitionNamesBySchema.TryGetValue(schema, out string schemaName))
            {
                return new JSchema
                {
                    ExtensionData = { { "$ref", $"#/definitions/{schemaName}" } }
                };
            }

            DeduplicateJsonSchemaFields(schema, context);

            return schema;
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/>, генерирует корневую схему и заполняет
        /// <see cref="SchemaContext.SchemasByContent"/> - созданные схемы контентов
        /// и <see cref="SchemaContext.RepeatedContents"/> - повторяющиеся контенты
        /// </summary>
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private JSchema GetSchemaRecursive(Content definition, SchemaContext context, bool forList)
        {
            if (context.SchemasByContent.ContainsKey(definition))
            {
                context.RepeatedContents.Add(definition);
                return context.SchemasByContent[definition];
            }
            
            var contentSchema = CreateContentSchema(definition);

            context.SchemasByContent[definition] = contentSchema;

            var qpFields = _fieldService.List(definition.ContentId).ToArray();

            contentSchema.Properties.Add(IdProp, new JSchema
            {
                Type = JSchemaType.Integer, Minimum = 0, ExclusiveMinimum = true
            });

            contentSchema.Required.Add(IdProp);

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
                    if (!context.ExcludeVirtualFields)
                    {
                        contentSchema.Properties[field.FieldName] = GetVirtualFieldSchema(
                            baseVirtualField, definition, context);
                    }
                }
                else if (!context.IgnoredFields.Contains(Tuple.Create(definition, field)))
                {
                    var qpField = field is BackwardRelationField
                        ? _fieldService.Read(field.FieldId)
                        : qpFields.SingleOrDefault(x => x.Id == field.FieldId);

                    if (qpField == null)
                    {
                        throw new InvalidOperationException($@"В definition указано поле id={
                            field.FieldId} которого нет в контенте id={definition.ContentId}");
                    }
                    
                    if (field is ExtensionField extensionField)
                    {
                        MergeExtensionFieldSchema(extensionField, qpField, contentSchema, context);
                    }
                    else
                    {
                        contentSchema.Properties[field.FieldName] = GetFieldSchema(field, qpField, context, forList);
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

        private JSchema CreateContentSchema(Content content)
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
            return schema;
        }

        private JSchema AttachFieldData(Quantumart.QP8.BLL.Field qpField, JSchema schema)
        {
            schema.Description =
                !IsHtmlWhiteSpace(qpField.Description)
                    ? qpField.Description
                    : !IsHtmlWhiteSpace(qpField.FriendlyName)
                        ? qpField.FriendlyName
                        : schema.Description;
            
            switch (qpField.ExactType)
            {
                case FieldExactTypes.StringEnum:
                    foreach (var item in qpField.StringEnumItems.Where(item => !item.Invalid))
                    {
                        schema.Enum.Add(item.Value);

                        if (item.IsDefault.GetValueOrDefault())
                        {
                            schema.Default = item.Value;
                        }
                    }
                    break;
            }
            
            return schema;
        }

        private static bool IsHtmlWhiteSpace(string str)
        {
            return String.IsNullOrEmpty(str) 
                || String.IsNullOrWhiteSpace(WebUtility.HtmlDecode(str));
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private void MergeExtensionFieldSchema(
            ExtensionField field, Quantumart.QP8.BLL.Field qpField, JSchema contentSchema, SchemaContext context)
        {
            JSchema extensionSchema = AttachFieldData(qpField, new JSchema { Type = JSchemaType.String });

            foreach (Content content in field.ContentMapping.Values)
            {
                var qpContent = _contentService.Read(content.ContentId);
                if (!String.IsNullOrWhiteSpace(qpContent.NetName))
                {
                    extensionSchema.Enum.Add(qpContent.NetName);
                }
            }

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

                        JSchema extFieldSchema = GetFieldSchema(extField, extQpField, context, false);
                        
                        return extFieldSchema;
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
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private JSchema GetFieldSchema(
            Field field, Quantumart.QP8.BLL.Field qpField, SchemaContext context, bool forList)
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
                JSchema backwardItemSchema = GetSchemaRecursive(backwardRelationalField.Content, context, forList);

                return AttachFieldData(qpField, new JSchema
                {
                    Type = JSchemaType.Array, Items = { backwardItemSchema }
                });
            }
            else if (field is EntityField entityField)
            {
                Content fieldContent = entityField.Content;

                if (qpField.RelationType == Quantumart.QP8.BLL.RelationType.OneToMany)
                {
                    return AttachFieldData(qpField, GetSchemaRecursive(fieldContent, context, forList));
                }
                else
                {
                    JSchema arrayItemSchema = GetSchemaRecursive(fieldContent, context, forList);

                    return AttachFieldData(qpField, new JSchema
                    {
                        Type = JSchemaType.Array, Items = { arrayItemSchema }
                    });
                }
            }
            else
            {
                throw new NotSupportedException($"Поля типа {field.GetType()} не поддерживается");
            }
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private JSchema GetVirtualFieldSchema(
            BaseVirtualField baseVirtualField, Content definition, SchemaContext context)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                var boundFieldKey = Tuple.Create(definition, virtualMultiEntityField.Path);

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
                    var virtualFieldKey = Tuple.Create(definition, virtualField.Path);

                    return GetFieldSchema(context.VirtualFields[virtualFieldKey], null, context, false);
                }
            }
            else
            {
                throw new NotSupportedException($"Поле типа {baseVirtualField.GetType()} не поддерживается");
            }
        }

        /// <exception cref="NotSupportedException" />
        private static JSchemaType ConvertTypeToJsType(Type type)
        {
            if (type == typeof(bool))
                return JSchemaType.Boolean;

            if (type == typeof(string))
                return JSchemaType.String;

            if (type == typeof(int))
                return JSchemaType.Integer;

            throw new NotSupportedException("Схема не поддерживает конвертер возвращающий тип " + type);
        }
        
        private JSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field qpField)
        {
            var schema = new JSchema();

            switch (qpField.ExactType)
            {
                case FieldExactTypes.Numeric:
                    schema.Type = qpField.IsInteger ? JSchemaType.Integer : JSchemaType.Number;
                    break;

                case FieldExactTypes.File:
                    schema.Type = JSchemaType.Object;
                    schema.AllowAdditionalProperties = false;

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

            return AttachFieldData(qpField, schema);
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

        public Dictionary<string, object> ConvertArticle(Article article, IArticleFilter filter)
        {
            if (article == null || !article.Visible || article.Archived || !filter.Matches(article))
            {
                return null;
            }

            var dict = new Dictionary<string, object> { { IdProp, article.Id } };

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
            Dictionary<string, object> dict, ExtensionArticleField field, IArticleFilter filter)
        {
            if (field.Item == null)
            {
                return;
            }

            dict[field.FieldName] = field.Item.ContentName;

            foreach (ArticleField childField in field.Item.Fields.Values)
            {
                object value = ConvertField(childField, filter);

                if (value != null && !(value is string && string.IsNullOrEmpty((string)value)))
                {
                    dict[childField.FieldName] = value;
                }
            }
        }

        /// <exception cref="NotSupportedException" />
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

            throw new NotSupportedException($"Поле типа {field.GetType()} не поддерживается");
        }
    }
}
