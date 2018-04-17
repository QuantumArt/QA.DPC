using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Constants;

namespace QA.Core.DPC.Formatters.Services
{
    public class EditorSchemaFormatter : IFormatter<Content>
    {
        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly VirtualFieldPathEvaluator _virtualFieldPathEvaluator;

        public EditorSchemaFormatter(
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldPathEvaluator virtualFieldPathEvaluator)
        {
            _contentService = contentService;
            _fieldService = fieldService;
            _virtualFieldPathEvaluator = virtualFieldPathEvaluator;
        }

        #region Models

        private class ProductSchema
        {
            public IContentSchema Content { get; set; }

            public Dictionary<string, ContentSchema> Definitions { get; set; }
                = new Dictionary<string, ContentSchema>();
        }

        public interface IContentSchema { }

        private class ContentSchema : IContentSchema
        {
            public int ContentId { get; set; }
            public string ContentPath { get; set; }
            public string ContentName { get; set; }
            public string ContentTitle { get; set; }
            public string ContentDescription { get; set; }

            public Dictionary<string, FieldSchema> Fields { get; set; }
                = new Dictionary<string, FieldSchema>();
        }

        private class ContentSchemaRef : IContentSchema
        {
            [JsonProperty("$ref")]
            public string Ref { get; set; }
        }
        
        private class FieldSchema
        {
            public int FieldId { get; set; }
            public string FieldName { get; set; }
            public string FieldTitle { get; set; }
            public string FieldDescription { get; set; }
            public int FieldOrder { get; set; }
            public bool IsRequired { get; set; }
            public FieldExactTypes FieldType { get; set; }
        }

        private class EnumFieldSchema : FieldSchema
        {
            public StringEnumItem[] Items { get; set; } = new StringEnumItem[0];
        }

        private class RelationFieldSchema : FieldSchema
        {
            public IContentSchema Content { get; set; }
        }

        private class ExtensionFieldSchema : FieldSchema
        {
            public Dictionary<string, IContentSchema> Contents { get; set; }
                = new Dictionary<string, IContentSchema>();
        }
        
        #endregion

        #region IFormatter

        public Task<Content> Read(Stream stream)
        {
            throw new NotSupportedException();
        }

        public async Task Write(Stream stream, Content content)
        {
            _contentService.LoadStructureCache();
            _fieldService.LoadStructureCache();

            string schema = GetSchema(content);

            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(schema);
                await writer.FlushAsync();
            }
        }

        #endregion

        private class SchemaContext
        {
            /// <summary>
            /// Значения вирутальных полей
            /// </summary>
            public Dictionary<Tuple<Content, string>, Field> VirtualFields
                = new Dictionary<Tuple<Content, string>, Field>();

            /// <summary>
            /// Cписок виртуальных полей которые надо игнорировать при создании схемы
            /// </summary>
            public List<Tuple<Content, Field>> IgnoredFields = new List<Tuple<Content, Field>>();

            /// <summary>
            /// Созданные схемы для различных контентов
            /// </summary>
            public Dictionary<Content, ContentSchema> SchemasByContent
                = new Dictionary<Content, ContentSchema>();

            /// <summary>
            /// Набор схем контентов, которые повторяются при обходе <see cref="Content"/>
            /// </summary>
            public HashSet<ContentSchema> RepeatedSchemas = new HashSet<ContentSchema>();

            /// <summary>
            /// Имена ссылок на повторяющиеся контенты
            /// </summary>
            public Dictionary<ContentSchema, string> DefinitionNamesBySchema
                = new Dictionary<ContentSchema, string>();
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public string GetSchema(Content content, bool prettyPrint = true)
        {
            var context = new SchemaContext();

            FillVirtualFieldsInfo(content, context);

            ContentSchema contentSchema = GetContentSchema(content, context, "");

            ProductSchema productSchema = GetProductSchema(contentSchema, context);

            return JsonConvert.SerializeObject(productSchema, new JsonSerializerSettings
            {
                // ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = prettyPrint
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None,

                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new StringEnumConverter() }
            });
        }
        
        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/> и заполняет
        /// <see cref="SchemaContext.IgnoredFields"/> - список полей которые надо игнорировать при создании схемы
        /// и <see cref="SchemaContext.VirtualFields"/> - значения вирутальных полей
        /// </summary>
        private void FillVirtualFieldsInfo(
            Content content,
            SchemaContext context,
            HashSet<Content> visitedContents = null)
        {
            if (visitedContents == null)
            {
                visitedContents = new HashSet<Content>();
            }
            else if (visitedContents.Contains(content))
            {
                return;
            }
            visitedContents.Add(content);

            foreach (Field field in content.Fields)
            {
                if (field is BaseVirtualField baseField)
                {
                    ProcessVirtualField(baseField, content, context);
                }
                else if (field is EntityField entityField)
                {
                    FillVirtualFieldsInfo(entityField.Content, context, visitedContents);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (Content extContent in extensionField.ContentMapping.Values)
                    {
                        FillVirtualFieldsInfo(extContent, context, visitedContents);
                    }
                }
            }
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/> и заполняет
        /// <see cref="SchemaContext.IgnoredFields"/> - список полей которые надо игнорировать при создании схемы
        /// и <see cref="SchemaContext.VirtualFields"/> - значения вирутальных полей
        /// </summary>
        /// <exception cref="InvalidOperationException" />
        private void ProcessVirtualField(
            BaseVirtualField baseVirtualField,
            Content definition,
            SchemaContext context)
        {
            if (baseVirtualField is VirtualMultiEntityField virtualMultiEntityField)
            {
                string path = virtualMultiEntityField.Path;

                var virtualFieldKey = Tuple.Create(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                Field foundField = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out bool hasFilter, out Content parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(Tuple.Create(parent, foundField));
                }

                context.VirtualFields[virtualFieldKey] = foundField;

                if (foundField is EntityField foundEntityField)
                {
                    foreach (BaseVirtualField childField in virtualMultiEntityField.Fields)
                    {
                        ProcessVirtualField(childField, foundEntityField.Content, context);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        "В Path VirtualMultiEntityField должны быть только поля EntityField или наследники");
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

                var virtualFieldKey = Tuple.Create(definition, path);

                if (context.VirtualFields.ContainsKey(virtualFieldKey))
                {
                    return;
                }

                Field fieldToMove = _virtualFieldPathEvaluator
                    .GetFieldByPath(path, definition, out bool hasFilter, out Content parent);

                if (!hasFilter)
                {
                    context.IgnoredFields.Add(Tuple.Create(parent, fieldToMove));
                }

                context.VirtualFields[virtualFieldKey] = fieldToMove;
            }
        }

        /// <summary>
        /// Рекурсивно обходит <see cref="Content"/>, генерирует корневую схему и заполняет
        /// <see cref="SchemaContext.SchemasByContent"/> - созданные схемы контентов
        /// и <see cref="SchemaContext.RepeatedContents"/> - повторяющиеся контенты
        /// </summary>
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private ContentSchema GetContentSchema(Content content, SchemaContext context, string path)
        {
            path += $"/{content.ContentId}";

            if (context.SchemasByContent.ContainsKey(content))
            {
                ContentSchema repeatedSchema = context.SchemasByContent[content];
                context.RepeatedSchemas.Add(repeatedSchema);
                return repeatedSchema;
            }

            ContentSchema contentSchema = CreateContentSchema(content, path);

            context.SchemasByContent[content] = contentSchema;

            var qpFields = _fieldService.List(content.ContentId).ToArray();

            var fieldsToAdd = content.Fields
                .Where(field => !(field is Dictionaries || field is BaseVirtualField)
                    && !context.IgnoredFields.Contains(Tuple.Create(content, field)));

            foreach (Field field in fieldsToAdd)
            {
                if (field.FieldName == null)
                {
                    throw new InvalidOperationException(
                        $"FieldName is null: {new { field.FieldId, field.FieldName }}");
                }

                var qpField = field is BackwardRelationField
                        ? _fieldService.Read(field.FieldId)
                        : qpFields.SingleOrDefault(f => f.Id == field.FieldId);

                if (qpField == null)
                {
                    throw new InvalidOperationException($@"В definition указано поле id={
                        field.FieldId} которого нет в контенте id={content.ContentId}");
                }

                contentSchema.Fields[field.FieldName] = GetFieldSchema(field, qpField, context, path);
            }

            if (content.LoadAllPlainFields)
            {
                var qpFieldsToAdd = qpFields
                    .Where(qpField => qpField.RelationType == Quantumart.QP8.BLL.RelationType.None
                        && content.Fields.All(field => field.FieldId != qpField.Id)
                        && !context.IgnoredFields
                            .Any(tuple => tuple.Item1.Equals(content)
                                && tuple.Item2.FieldId == qpField.Id));

                foreach (var qpField in qpFieldsToAdd)
                {
                    contentSchema.Fields[qpField.Name] = GetFieldSchema(null, qpField, context, path);
                }
            }
            
            return contentSchema;
        }

        private ContentSchema CreateContentSchema(Content content, string path)
        {
            var qpContent = _contentService.Read(content.ContentId);

            return new ContentSchema
            {
                ContentId = qpContent.Id,
                ContentPath = path,
                ContentName = String.IsNullOrWhiteSpace(qpContent.NetName) ? "" : qpContent.NetName,
                ContentTitle = IsHtmlWhiteSpace(qpContent.Name) ? "" : qpContent.Name,
                ContentDescription = IsHtmlWhiteSpace(qpContent.Description) ? "" : qpContent.Description,
            };
        }
        
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private FieldSchema GetFieldSchema(
            Field field, Quantumart.QP8.BLL.Field qpField, SchemaContext context, string path)
        {
            path += $":{field?.FieldId ?? qpField.Id}";
            
            FieldSchema fieldSchema;
            
            if (field == null || field is PlainField)
            {
                fieldSchema = GetPlainFieldSchema(qpField);
            }
            else if (field is EntityField entityField)
            {
                fieldSchema = new RelationFieldSchema
                {
                    Content = GetContentSchema(entityField.Content, context, path)
                };
            }
            else if (field is ExtensionField extensionField)
            {
                fieldSchema = GetExtensionFieldSchema(extensionField, context, path);
            }
            else
            {
                throw new NotSupportedException($"Поля типа {field.GetType()} не поддерживается");
            }

            fieldSchema.FieldId = field?.FieldId ?? qpField.Id;
            fieldSchema.FieldName = field?.FieldName ?? qpField.Name ?? "";
            fieldSchema.FieldOrder = qpField.Order;
            fieldSchema.FieldType = qpField.ExactType;
            fieldSchema.FieldTitle = IsHtmlWhiteSpace(qpField.FriendlyName) ? "" : qpField.FriendlyName;
            fieldSchema.FieldDescription = IsHtmlWhiteSpace(qpField.Description) ? "" : qpField.Description;

            // TODO: fieldSchema.IsRequired

            return fieldSchema;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private FieldSchema GetExtensionFieldSchema(
            ExtensionField extensionField, SchemaContext context, string path)
        {
            var contentSchemas = new Dictionary<string, IContentSchema>();

            foreach (Content content in extensionField.ContentMapping.Values)
            {
                var qpContent = _contentService.Read(content.ContentId);

                if (!String.IsNullOrEmpty(qpContent.NetName)
                    && !contentSchemas.ContainsKey(qpContent.NetName))
                {
                    contentSchemas[qpContent.NetName] = GetContentSchema(content, context, path);
                }
            }

            return new ExtensionFieldSchema { Contents = contentSchemas };
        }

        private static FieldSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field qpField)
        {
            if (qpField.ExactType == FieldExactTypes.StringEnum)
            {
                return new EnumFieldSchema { Items = qpField.StringEnumItems.ToArray() };
            }

            // TODO: other field types

            return new FieldSchema();
        }

        private ProductSchema GetProductSchema(ContentSchema contentSchema, SchemaContext context)
        {
            context.DefinitionNamesBySchema = context.RepeatedSchemas
                .GroupBy(schema => String.IsNullOrEmpty(schema.ContentName)
                    ? $"Content{schema.ContentId}" 
                    : schema.ContentName)
                .SelectMany(group => group.Select((schema, i) => new
                {
                    Key = schema,
                    Value = $"{group.Key}{(i == 0 ? "" : i.ToString())}",
                }))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new ProductSchema
            {
                Content = DeduplicateContentSchema(contentSchema, context, new HashSet<ContentSchema>()),

                Definitions = context.DefinitionNamesBySchema
                    .ToDictionary(pair => pair.Value, pair => pair.Key),
            };
        }

        private IContentSchema DeduplicateContentSchema(
            IContentSchema schema, SchemaContext context, HashSet<ContentSchema> visitedSchemas)
        {
            if (schema is ContentSchema contentSchema)
            {
                if (visitedSchemas.Contains(contentSchema))
                {
                    return GetSchemaRef(contentSchema, context);
                }

                visitedSchemas.Add(contentSchema);
                
                foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
                {
                    if (fieldSchema is RelationFieldSchema relationSchema)
                    {
                        relationSchema.Content = DeduplicateContentSchema(
                            relationSchema.Content, context, visitedSchemas);
                    }
                    else if (fieldSchema is ExtensionFieldSchema extensionSchema)
                    {
                        foreach (var pair in extensionSchema.Contents.ToArray())
                        {
                            extensionSchema.Contents[pair.Key] = DeduplicateContentSchema(
                                pair.Value, context, visitedSchemas);
                        }
                    }
                }

                if (context.RepeatedSchemas.Contains(contentSchema))
                {
                    return GetSchemaRef(contentSchema, context);
                }
            }

            return schema;
        }

        private ContentSchemaRef GetSchemaRef(ContentSchema contentSchema, SchemaContext context)
        {
            string name = context.DefinitionNamesBySchema[contentSchema];

            return new ContentSchemaRef
            {
                Ref = $"#/{nameof(ProductSchema.Definitions)}/{name}"
            };
        }

        #region Utils

        private static bool IsHtmlWhiteSpace(string str)
        {
            return String.IsNullOrEmpty(str)
                || String.IsNullOrWhiteSpace(WebUtility.HtmlDecode(str));
        }

        #endregion
    }
}
