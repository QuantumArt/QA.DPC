using QA.Core.DPC.QP.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using Quantumart.QP8.BLL.Services.API;
using Quantumart.QP8.Constants;
using Quantumart.QPublishing.Database;
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
        private readonly DBConnector _dbConnector;
        private readonly ContentService _contentService;
        private readonly FieldService _fieldService;
        private readonly VirtualFieldContextService _virtualFieldContextService;

        public EditorSchemaService(
            IConnectionProvider connectionProvider,
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService)
        {
            _dbConnector = new DBConnector(connectionProvider.GetConnection());
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
                if (qpField.ExactType != FieldExactTypes.DynamicImage)
                {
                    contentSchema.Fields[field.FieldName] = GetFieldSchema(field, qpField, context, path);
                }
            }

            if (content.LoadAllPlainFields)
            {
                var qpFieldsToAdd = qpFields
                    .Where(qpField => qpField.RelationType == Quantumart.QP8.BLL.RelationType.None
                        && qpField.ExactType != FieldExactTypes.DynamicImage
                        && content.Fields.All(field => field.FieldId != qpField.Id)
                        && !context.IgnoredFields
                            .Any(tuple => tuple.Item1.Equals(content)
                                && tuple.Item2.FieldId == qpField.Id));

                foreach (var qpField in qpFieldsToAdd)
                {
                    contentSchema.Fields[qpField.Name] = GetFieldSchema(null, qpField, context, path);
                }
            }

            contentSchema.DisplayFieldName = contentSchema.Fields.Values
                .OfType<PlainFieldSchema>()
                .Where(f => f.FieldType == FieldExactTypes.String)
                .OrderByDescending(f => f.ViewInList)
                .ThenBy(f => f.FieldOrder)
                .Select(f => f.FieldName)
                .FirstOrDefault();

            return contentSchema;
        }

        private ContentSchema CreateContentSchema(Content content, string path)
        {
            var qpContent = _contentService.Read(content.ContentId);

            return new ContentSchema
            {
                ContentId = qpContent.Id,
                ContentPath = path == "" ? "/" : path,
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
            path += $"/{field?.FieldName ?? qpField.Name ?? ""}";

            FieldSchema fieldSchema;

            if (field == null || field is PlainField)
            {
                fieldSchema = GetPlainFieldSchema(qpField);
            }
            else if (field is EntityField entityField)
            {
                fieldSchema = GetRelationFieldSchema(entityField, qpField, context, path);
            }
            else if (field is ExtensionField extensionField)
            {
                fieldSchema = GetExtensionFieldSchema(extensionField, qpField, context, path);
            }
            else
            {
                throw new NotSupportedException($"Поле типа {field.GetType()} не поддерживается");
            }

            fieldSchema.FieldId = field?.FieldId ?? qpField.Id;
            fieldSchema.FieldName = field?.FieldName ?? qpField.Name ?? "";
            fieldSchema.FieldOrder = qpField.Order;
            fieldSchema.FieldType = qpField.ExactType;
            fieldSchema.FieldDescription = IsHtmlWhiteSpace(qpField.Description) ? "" : qpField.Description;

            if (field is BackwardRelationField backwardField && !IsHtmlWhiteSpace(backwardField.DisplayName))
            {
                fieldSchema.FieldTitle = backwardField.DisplayName;
            }
            else if (!IsHtmlWhiteSpace(qpField.FriendlyName))
            {
                fieldSchema.FieldTitle = qpField.FriendlyName;
            }
            else
            {
                fieldSchema.FieldTitle = "";
            }

            fieldSchema.IsRequired = qpField.Required && !(field is BackwardRelationField);
            fieldSchema.IsReadOnly = qpField.ReadOnly;
            fieldSchema.ViewInList = qpField.ViewInList;
            fieldSchema.DefaultValue = GetDefaultValue(qpField);

            return fieldSchema;
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private RelationFieldSchema GetRelationFieldSchema(
            EntityField entityField, Quantumart.QP8.BLL.Field qpField, SchemaContext context, string path)
        {
            ContentSchema contentSchema = GetContentSchema(entityField.Content, context, path);

            string relationCondition = null;
            if (!String.IsNullOrWhiteSpace(entityField.RelationCondition))
            {
                relationCondition = entityField.RelationCondition;
            } 
            else if (!(entityField is BackwardRelationField)
                && qpField.UseRelationCondition
                && !String.IsNullOrWhiteSpace(qpField.RelationCondition))
            {
                relationCondition = qpField.RelationCondition;
            }

            string[] displayFieldNames = contentSchema.Fields.Values
                .OfType<PlainFieldSchema>()
                .Where(f => f.FieldType != FieldExactTypes.Textbox && f.FieldType != FieldExactTypes.VisualEdit)
                .OrderByDescending(f => f.ViewInList)
                .ThenBy(f => f.FieldOrder)
                .Take(Math.Max(qpField.ListFieldTitleCount, 1))
                .Select(f => f.FieldName)
                .ToArray();

            if (qpField.ExactType == FieldExactTypes.O2MRelation && !(entityField is BackwardRelationField)
                || qpField.ExactType == FieldExactTypes.M2ORelation && entityField is BackwardRelationField)
            {
                return new SingleRelationFieldSchema
                {
                    RelatedContent = contentSchema,
                    CloningMode = entityField.CloningMode,
                    UpdatingMode = entityField.UpdatingMode,
                    IsDpcBackwardField = entityField is BackwardRelationField,
                    RelationCondition = relationCondition,
                    DisplayFieldNames = displayFieldNames
                };
            }
            if (qpField.ExactType == FieldExactTypes.M2MRelation
                || qpField.ExactType == FieldExactTypes.O2MRelation && entityField is BackwardRelationField
                || qpField.ExactType == FieldExactTypes.M2ORelation && !(entityField is BackwardRelationField))
            {
                int? orderFieldId = qpField.TreeOrderFieldId ?? qpField.ListOrderFieldId ?? qpField.OrderFieldId;
                bool orderByTitle = qpField.TreeOrderByTitle || qpField.ListOrderByTitle || qpField.OrderByTitle;

                string orderByFieldName = contentSchema.Fields.Values
                    .OfType<PlainFieldSchema>()
                    .Where(f => f.FieldId == orderFieldId)
                    .Select(f => f.FieldName)
                    .FirstOrDefault();

                if (orderByFieldName == null && orderByTitle)
                {
                    orderByFieldName = displayFieldNames.FirstOrDefault();
                }

                int? maxDataListItemCount = null;

                if (qpField.ExactType == FieldExactTypes.M2ORelation && qpField.BackRelationId != null)
                {
                    // MaxDataListItemCount лежит в соотв. поле O2MRelation
                    var reverseField = _fieldService.Read(qpField.BackRelationId.Value);
                    if (reverseField.MaxDataListItemCount > 0)
                    {
                        maxDataListItemCount = reverseField.MaxDataListItemCount;
                    }
                }
                else if (qpField.MaxDataListItemCount > 0)
                {
                    maxDataListItemCount = qpField.MaxDataListItemCount;
                }

                return new MultiRelationFieldSchema
                {
                    RelatedContent = contentSchema,
                    CloningMode = entityField.CloningMode,
                    UpdatingMode = entityField.UpdatingMode,
                    IsDpcBackwardField = entityField is BackwardRelationField,
                    RelationCondition = relationCondition,
                    DisplayFieldNames = displayFieldNames,
                    OrderByFieldName = orderByFieldName,
                    MaxDataListItemCount = maxDataListItemCount
                };
            }

            throw new NotSupportedException($"Связь типа {qpField.ExactType} не поддерживается");
        }

        /// <exception cref="NotSupportedException" />
        /// <exception cref="InvalidOperationException" />
        private FieldSchema GetExtensionFieldSchema(
            ExtensionField extensionField, Quantumart.QP8.BLL.Field qpField, SchemaContext context, string path)
        {
            var contentSchemas = new Dictionary<string, IContentSchema>();

            foreach (Content content in extensionField.ContentMapping.Values)
            {
                var qpContent = _contentService.Read(content.ContentId);

                if (!String.IsNullOrEmpty(qpContent.NetName)
                    && !contentSchemas.ContainsKey(qpContent.NetName))
                {
                    var contentSchema = GetContentSchema(content, context, $"{path}/{qpContent.NetName}");
                    contentSchema.ForExtension = true;
                    contentSchemas[qpContent.NetName] = contentSchema;
                }
            }

            return new ExtensionFieldSchema
            {
                Changeable = qpField.Changeable,
                ExtensionContents = contentSchemas
            };
        }

        private PlainFieldSchema GetPlainFieldSchema(Quantumart.QP8.BLL.Field qpField)
        {
            switch (qpField.ExactType)
            {
                case FieldExactTypes.String:
                    return new StringFieldSchema { RegexPattern = qpField.InputMask };
                
                case FieldExactTypes.Numeric:
                    return new NumericFieldSchema { IsInteger = qpField.IsInteger };

                case FieldExactTypes.Classifier:
                    return new ClassifierFieldSchema { Changeable = qpField.Changeable };

                case FieldExactTypes.File:
                case FieldExactTypes.Image:
                    return new FileFieldSchema
                    {
                        UseSiteLibrary = qpField.UseSiteLibrary,
                        // FolderUrl = _dbConnector.GetUrlForFileAttribute(qpField.Id, true, true)
                    };

                case FieldExactTypes.StringEnum:
                    return new EnumFieldSchema
                    {
                        ShowAsRadioButtons = qpField.ShowAsRadioButtons,
                        Items = qpField.StringEnumItems.ToArray()
                    };
                
                // TODO: other plain field types

                default:
                    return new PlainFieldSchema();
            }
        }

        private object GetDefaultValue(Quantumart.QP8.BLL.Field qpField)
        {
            switch (qpField.ExactType)
            {
                case FieldExactTypes.String:
                case FieldExactTypes.Textbox:
                case FieldExactTypes.VisualEdit:
                case FieldExactTypes.StringEnum:
                case FieldExactTypes.File:
                case FieldExactTypes.Image:
                    return qpField.DefaultValue;

                case FieldExactTypes.Boolean:
                    return qpField.DefaultValue == "1" ? true : (object)null;

                case FieldExactTypes.Numeric:
                    return Double.TryParse(qpField.DefaultValue, out double number)
                        ? qpField.IsInteger ? (int)number : number : (object)null;

                case FieldExactTypes.Date:
                case FieldExactTypes.Time:
                case FieldExactTypes.DateTime:
                    return DateTime.TryParse(qpField.DefaultValue, out DateTime dateTime)
                        ? dateTime : (object)null;

                case FieldExactTypes.Classifier:
                    return Int32.TryParse(qpField.DefaultValue, out int contentId)
                        ? _contentService.Read(contentId).NetName : null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Построить схему корневого DPC-контента и набор ссылок на повторяющиеся контенты
        /// </summary>
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

        /// <summary>
        /// Заменить повторяющиеся объекты <see cref="ContentSchema"/> на ссылки <see cref="ContentSchemaJsonRef"/>
        /// </summary>
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
                        relationSchema.RelatedContent = DeduplicateContentSchema(
                            relationSchema.RelatedContent, context, visitedSchemas);
                    }
                    else if (fieldSchema is ExtensionFieldSchema extensionSchema)
                    {
                        foreach (var pair in extensionSchema.ExtensionContents.ToArray())
                        {
                            extensionSchema.ExtensionContents[pair.Key] = DeduplicateContentSchema(
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

            if (!contentSchema.ForExtension)
            {
                // Один и тот же контент может использоваться как Extension и как Relation
                // в разных частях схемы. В этом случае считаем, что контент не является Extension.
                mergedContentSchema.ForExtension = false;
            }

            foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
            {
                // Перезаписываем все сложные поля, потому что для одного и того же контента,
                // в одном месте ProductDefinition заданное поле может быть RelationField | ExtensionField,
                // а в другом месте у контента стоит флаг LoadAllPlainFields,
                // и поле является PlainField c типом FieldExactTyes.O2MRelation | FieldExactTyes.Classifier
                if (fieldSchema is ExtensionFieldSchema extFieldSchema)
                {
                    var copy = extFieldSchema.ShallowCopy();

                    copy.ExtensionContents = extFieldSchema.ExtensionContents.ToDictionary(
                        pair => pair.Key,
                        pair => (IContentSchema)new ContentSchemaIdRef
                        {
                            ContentId = pair.Value.ContentId,
                        });

                    // Объединяем наборы допустимых контентов
                    if (mergedContentSchema.Fields
                            .TryGetValue(fieldSchema.FieldName, out FieldSchema mergedFieldSchema)
                        && mergedFieldSchema is ExtensionFieldSchema mergedExtensionSchema)
                    {
                        foreach (var pair in mergedExtensionSchema.ExtensionContents)
                        {
                            if (!copy.ExtensionContents.ContainsKey(pair.Key))
                            {
                                copy.ExtensionContents[pair.Key] = pair.Value;
                            }
                        }
                    }

                    mergedContentSchema.Fields[fieldSchema.FieldName] = copy;
                }
                else if (fieldSchema is SingleRelationFieldSchema singleFieldSchema)
                {
                    var copy = singleFieldSchema.ShallowCopy();
                    copy.RelatedContent = new ContentSchemaIdRef
                    {
                        ContentId = singleFieldSchema.RelatedContent.ContentId,
                    };
                    mergedContentSchema.Fields[fieldSchema.FieldName] = copy;
                }
                else if (fieldSchema is MultiRelationFieldSchema multiFieldSchema)
                {
                    var copy = multiFieldSchema.ShallowCopy();
                    copy.RelatedContent = new ContentSchemaIdRef
                    {
                        ContentId = multiFieldSchema.RelatedContent.ContentId,
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
                if (fieldSchema is RelationFieldSchema relationFieldSchema)
                {
                    if (relationFieldSchema.RelatedContent is ContentSchema childContentSchema)
                    {
                        action.Invoke(childContentSchema, schemasByContentId);
                    }
                }
                else if (fieldSchema is ExtensionFieldSchema extensionFieldSchema)
                {
                    foreach (var childContentSchema in extensionFieldSchema.ExtensionContents.Values.OfType<ContentSchema>())
                    {
                        action.Invoke(childContentSchema, schemasByContentId);
                    }
                }
            }
        }
    }
}
