using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.Core.Models.Tools;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QA.Core.DPC.Formatters.Services
{
    public class EditorSchemaFormatter : IFormatter<Content>
    {
        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly VirtualFieldContextService _virtualFieldContextService;

        public EditorSchemaFormatter(
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService)
        {
            _contentService = contentService;
            _fieldService = fieldService;
            _virtualFieldContextService = virtualFieldContextService;
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
            /// Cписок виртуальных полей которые надо игнорировать при создании схемы
            /// </summary>
            public List<Tuple<Content, Field>> IgnoredFields;

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
            if (content == null) throw new ArgumentNullException(nameof(content));

            VirtualFieldContext virtualFieldContext = _virtualFieldContextService.GetVirtualFieldContext(content);

            var context = new SchemaContext
            {
                IgnoredFields = virtualFieldContext.IgnoredFields,
            };

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

        #region PartialContent

        private class PartialContentContext
        {
            public ReferenceDictionary<Content, Content> ContentPrototypesByReference;

            public Dictionary<Content, Content> ContentPrototypesByValue;

            public ReferenceHashSet<Content> VisitedContents;

            public HashSet<string> SelectedFieldPaths;
        }

        private static readonly Regex PathRegex
            = new Regex("^(/[1-9][0-9]*:[1-9][0-9]*)*/[1-9][0-9]*$", RegexOptions.Compiled);

        /// <exception cref="InvalidOperationException" />
        public Content GetPartialContent(Content rootContent, string[] contentPaths)
        {
            if (rootContent == null) throw new ArgumentNullException(nameof(rootContent));
            if (contentPaths == null) throw new ArgumentNullException(nameof(contentPaths));
            if (contentPaths.Length == 0) throw new ArgumentException("Paths should not be empty", nameof(contentPaths));

            for (int i = 0; i < contentPaths.Length; i++)
            {
                if (!PathRegex.IsMatch(contentPaths[i]))
                {
                    throw new InvalidOperationException($"Path [{i}] \"{contentPaths[i]}\" is invalid");
                }
            }

            var context = new PartialContentContext();

            // клонируем rootContent, потому что его описание будет изменено в RemoveNotSelectedFields
            rootContent = rootContent.DeepCopy();

            // находим дубликаты контентов и сохраняем в словаре
            context.ContentPrototypesByReference = new ReferenceDictionary<Content, Content>();
            context.ContentPrototypesByValue = new Dictionary<Content, Content>();
            FillContentPrototypesDictionary(rootContent, context);

            // заменяем дубликаты контентов на их прообразы
            context.VisitedContents = new ReferenceHashSet<Content>();
            DecuplicateContents(rootContent, context);

            // находим контент по пути
            Content foundContent = FindContentByPath(rootContent, contentPaths[0]);

            // удаляем все связи между контентами, кроме описанных в contentPaths
            context.VisitedContents = new ReferenceHashSet<Content>();
            context.SelectedFieldPaths = new HashSet<string>(contentPaths.Skip(1).Select(GetFieldPath));
            RemoveNotSelectedFields(rootContent, context, "");

            return foundContent;
        }

        private static string GetFieldPath(string fieldContentPath)
        {
            return fieldContentPath.Substring(0, fieldContentPath.LastIndexOf('/'));
        }

        private void FillContentPrototypesDictionary(Content content, PartialContentContext context)
        {
            if (context.ContentPrototypesByReference.ContainsKey(content))
            {
                return;
            }
            if (context.ContentPrototypesByValue.ContainsKey(content))
            {
                context.ContentPrototypesByReference[content] = context.ContentPrototypesByValue[content];
            }
            else
            {
                context.ContentPrototypesByValue[content] = content;
                context.ContentPrototypesByReference[content] = content;
            }

            foreach (Association field in content.Fields.OfType<Association>())
            {
                foreach (Content childContent in field.Contents)
                {
                    FillContentPrototypesDictionary(childContent, context);
                }
            }
        }

        private void DecuplicateContents(Content content, PartialContentContext context)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            foreach (Field field in content.Fields)
            {
                if (field is EntityField entityField)
                {
                    entityField.Content = context.ContentPrototypesByReference[entityField.Content];

                    DecuplicateContents(entityField.Content, context);
                }
                else if (field is ExtensionField extensionField)
                {
                    foreach (var pair in extensionField.ContentMapping.ToArray())
                    {
                        Content childContent = context.ContentPrototypesByReference[pair.Value];

                        extensionField.ContentMapping[pair.Key] = childContent;

                        DecuplicateContents(childContent, context);
                    }
                }
            }
        }

        /// <exception cref="InvalidOperationException" />
        private Content FindContentByPath(Content content, string contentPath)
        {
            var pathNotFound = new InvalidOperationException($"Content not found for path \"{contentPath}\"");

            string[] pathSegments = contentPath.Split(':');

            string[] rootContent = pathSegments[0].Split('/');
            int rootContentId = Int32.Parse(rootContent[1]);
            if (content.ContentId != rootContentId)
            {
                throw pathNotFound;
            }

            foreach (string pathSegment in pathSegments.Skip(1))
            {
                string[] fieldContent = pathSegment.Split('/');
                int fieldId = Int32.Parse(fieldContent[0]);
                int contentId = Int32.Parse(fieldContent[1]);

                Field field = content.Fields.FirstOrDefault(f => f.FieldId == fieldId);

                if (field is EntityField entityField)
                {
                    content = entityField.Content;

                    if (content.ContentId != contentId)
                    {
                        throw pathNotFound;
                    }
                }
                else if (field is ExtensionField extensionField)
                {
                    content = extensionField.ContentMapping.Values
                        .FirstOrDefault(c => c.ContentId == contentId);

                    if (content == null)
                    {
                        throw pathNotFound;
                    }
                }
                else
                {
                    throw pathNotFound;
                }
            }

            return content;
        }

        private void RemoveNotSelectedFields(Content content, PartialContentContext context, string fieldPath)
        {
            if (context.VisitedContents.Contains(content))
            {
                return;
            }

            context.VisitedContents.Add(content);

            string contentPath = fieldPath + $"/{content.ContentId}";

            foreach (Association field in content.Fields.OfType<Association>().ToArray())
            {
                fieldPath = contentPath + $":{field.FieldId}";

                if (!context.SelectedFieldPaths.Contains(fieldPath))
                {
                    content.Fields.Remove(field);
                }

                foreach (Content childContent in field.Contents)
                {
                    RemoveNotSelectedFields(childContent, context, fieldPath);
                }
            }
        }

        #endregion

        #region Utils

        private static bool IsHtmlWhiteSpace(string str)
        {
            return String.IsNullOrEmpty(str)
                || String.IsNullOrWhiteSpace(WebUtility.HtmlDecode(str));
        }

        #endregion
    }
}