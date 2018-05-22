using QA.Core.Models.Configuration;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Генерация схемы для редактирования контентов
    /// </summary>
    public class EditorSchemaService
    {
        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly VirtualFieldContextService _virtualFieldContextService;

        public EditorSchemaService(
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService)
        {
            _contentService = contentService;
            _fieldService = fieldService;
            _virtualFieldContextService = virtualFieldContextService;
        }

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
        
        /// <summary>
        /// Генерация схемы для редактирования контентов
        /// </summary>
        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        public ProductSchema GetProductSchema(Content content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));

            _contentService.LoadStructureCache();
            _fieldService.LoadStructureCache();

            VirtualFieldContext virtualFieldContext = _virtualFieldContextService.GetVirtualFieldContext(content);

            var context = new SchemaContext
            {
                IgnoredFields = virtualFieldContext.IgnoredFields,
            };

            ContentSchema contentSchema = GetContentSchema(content, context, "");

            ProductSchema productSchema = GetProductSchema(contentSchema, context);

            return productSchema;
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
            else if (field is BackwardRelationField backwardField)
            {
                fieldSchema = new BackwardFieldSchema
                {
                    Content = GetContentSchema(backwardField.Content, context, path)
                };
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
                    var contentSchema = GetContentSchema(content, context, path);
                    contentSchema.ForExtension = true;
                    contentSchemas[qpContent.NetName] = contentSchema;
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
                    if (fieldSchema is IRelationFieldSchema relationSchema)
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

        private ContentSchemaJsonRef GetSchemaRef(ContentSchema contentSchema, SchemaContext context)
        {
            string name = context.DefinitionNamesBySchema[contentSchema];

            return new ContentSchemaJsonRef
            {
                ContentId = contentSchema.ContentId,
                Ref = $"#/{nameof(ProductSchema.Definitions)}/{name}"
            };
        }

        private static bool IsHtmlWhiteSpace(string str)
        {
            return String.IsNullOrEmpty(str)
                || String.IsNullOrWhiteSpace(WebUtility.HtmlDecode(str));
        }

        /// <summary>
        /// Генерация словаря с объединенными схемами для каждого <see cref="ContentSchema.ContentId"/> из продукта
        /// </summary>
        public Dictionary<int, ContentSchema> GetMergedContentSchemas(ProductSchema productSchema)
        {
            var schemasByContentId = new Dictionary<int, ContentSchema>();

            if (productSchema.Content is ContentSchema)
            {
                CreateEmptyContentSchemas((ContentSchema)productSchema.Content, schemasByContentId);
            }
            foreach (ContentSchema definitionSchema in productSchema.Definitions.Values)
            {
                CreateEmptyContentSchemas(definitionSchema, schemasByContentId);
            }

            if (productSchema.Content is ContentSchema)
            {
                FillMergedContentSchemas((ContentSchema)productSchema.Content, schemasByContentId);
            }
            foreach (ContentSchema definitionSchema in productSchema.Definitions.Values)
            {
                FillMergedContentSchemas(definitionSchema, schemasByContentId);
            }

            return schemasByContentId;
        }

        /// <summary>
        /// Заполняем словарь пустыми схемами
        /// </summary>
        private void CreateEmptyContentSchemas(
            ContentSchema contentSchema, Dictionary<int, ContentSchema> shapesByContentId)
        {
            if (!shapesByContentId.ContainsKey(contentSchema.ContentId))
            {
                var copy = contentSchema.ShallowCopy();
                copy.Fields = new Dictionary<string, FieldSchema>();
                shapesByContentId[contentSchema.ContentId] = copy;
            }

            VisitChildSchemas(contentSchema, shapesByContentId, CreateEmptyContentSchemas);
        }

        /// <summary>
        /// Объединяем поля схем контентов
        /// </summary>
        private void FillMergedContentSchemas(
            ContentSchema contentSchema, Dictionary<int, ContentSchema> schemasByContentId)
        {
            ContentSchema mergedContentSchema = schemasByContentId[contentSchema.ContentId];

            foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
            {
                // Перезаписываем все сложные поля, потому что для одного и того же контента,
                // в одном месте ProductDefinition заданное поле может быть RelationField | ExtensionField,
                // а в другом месте у контента стоит флаг LoadAllPlainFields,
                // и поле является PlainField c типом FieldExactTyes.O2MRelation | FieldExactTyes.Classifier
                if (fieldSchema is ExtensionFieldSchema extFieldSchema)
                {
                    var copy = extFieldSchema.ShallowCopy();

                    copy.Contents = extFieldSchema.Contents.ToDictionary(
                        pair => pair.Key,
                        pair => (IContentSchema)new ContentSchemaIdRef
                        {
                            ContentId = pair.Value.ContentId,
                        });

                    mergedContentSchema.Fields[fieldSchema.FieldName] = copy;
                }
                else if (fieldSchema is RelationFieldSchema relFieldSchema)
                {
                    var copy = relFieldSchema.ShallowCopy();
                    copy.Content = new ContentSchemaIdRef
                    {
                        ContentId = relFieldSchema.Content.ContentId,
                    };
                    mergedContentSchema.Fields[fieldSchema.FieldName] = copy;
                }
                else if (fieldSchema is BackwardFieldSchema backFieldSchema)
                {
                    var copy = backFieldSchema.ShallowCopy();
                    copy.Content = new ContentSchemaIdRef
                    {
                        ContentId = backFieldSchema.Content.ContentId,
                    };
                    mergedContentSchema.Fields[fieldSchema.FieldName] = copy;
                }
                else if (!mergedContentSchema.Fields.ContainsKey(fieldSchema.FieldName))
                {
                    mergedContentSchema.Fields[fieldSchema.FieldName] = fieldSchema;
                }

                VisitChildSchemas(contentSchema, schemasByContentId, FillMergedContentSchemas);
            }
        }

        private void VisitChildSchemas(
            ContentSchema contentSchema,
            Dictionary<int, ContentSchema> schemasByContentId,
            Action<ContentSchema, Dictionary<int, ContentSchema>> action)
        {
            foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
            {
                if (fieldSchema is IRelationFieldSchema relationFieldSchema)
                {
                    if (relationFieldSchema.Content is ContentSchema childContentSchema)
                    {
                        action.Invoke(childContentSchema, schemasByContentId);
                    }
                }
                else if (fieldSchema is ExtensionFieldSchema extensionFieldSchema)
                {
                    foreach (var childContentSchema in extensionFieldSchema.Contents.Values.OfType<ContentSchema>())
                    {
                        action.Invoke(childContentSchema, schemasByContentId);
                    }
                }
            }
        }
    }
}
