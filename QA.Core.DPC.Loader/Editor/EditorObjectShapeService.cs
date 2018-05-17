using Quantumart.QP8.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.DPC.Loader.Editor
{
    /// <summary>
    /// Генерация словаря с формой пустого JSON-объекта для каждого ContentId из продукта
    /// </summary>
    public class EditorObjectShapeService
    {
        /// <summary>
        /// Генерация словаря с формой пустого JSON-объекта для каждого ContentId из продукта
        /// </summary>
        public Dictionary<int, ContentObject> GetContentShapes(ProductSchema productSchema)
        {
            var shapesByContentId = new Dictionary<int, ContentObject>();

            if (productSchema.Content is ContentSchema)
            {
                CreateContentShapes((ContentSchema)productSchema.Content, shapesByContentId);
            }
            foreach (ContentSchema definitionSchema in productSchema.Definitions.Values)
            {
                CreateContentShapes(definitionSchema, shapesByContentId);
            }

            if (productSchema.Content is ContentSchema)
            {
                FillContentShapes((ContentSchema)productSchema.Content, shapesByContentId);
            }
            foreach (ContentSchema definitionSchema in productSchema.Definitions.Values)
            {
                FillContentShapes(definitionSchema, shapesByContentId);
            }

            FixExtensionContentShapes(shapesByContentId);

            return shapesByContentId;
        }

        /// <summary>
        /// Заполняем словарь с формами JSON-объекта пустыми объектами
        /// </summary>
        private void CreateContentShapes(
            ContentSchema contentSchema, Dictionary<int, ContentObject> shapesByContentId)
        {
            if (!shapesByContentId.ContainsKey(contentSchema.ContentId))
            {
                shapesByContentId[contentSchema.ContentId] = new ContentObject
                {
                    [ContentObject.IdProp] = null,
                    [ContentObject.ContentIdProp] = contentSchema.ContentId,
                    [ContentObject.TimestampProp] = null,
                };
            }

            VisitChildSchemas(contentSchema, shapesByContentId, CreateContentShapes);
        }

        /// <summary>
        /// Заполняем поля JSON-объектов согласно схеме
        /// </summary>
        private void FillContentShapes(
            ContentSchema contentSchema, Dictionary<int, ContentObject> shapesByContentId)
        {
            ContentObject contentShape = shapesByContentId[contentSchema.ContentId];

            foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
            {
                if (!contentShape.ContainsKey(fieldSchema.FieldName))
                {
                    if (fieldSchema is ExtensionFieldSchema extensionFieldSchema)
                    {
                        contentShape[fieldSchema.FieldName] = new ExtensionFieldObject
                        {
                            Contents = extensionFieldSchema.Contents
                                .ToDictionary(c => c.Key, c => shapesByContentId[c.Value.ContentId])
                        };
                    }
                    else if (fieldSchema is BackwardFieldSchema backwardFieldSchema
                        || fieldSchema.FieldType == FieldExactTypes.M2ORelation
                        || fieldSchema.FieldType == FieldExactTypes.M2MRelation)
                    {
                        contentShape[fieldSchema.FieldName] = new object[0];
                    }
                    else if (fieldSchema is EnumFieldSchema enumFieldSchema)
                    {
                        contentShape[fieldSchema.FieldName] = enumFieldSchema.Items
                            .Where(i => i.IsDefault.GetValueOrDefault() && !i.Invalid)
                            .Select(i => i.Value)
                            .FirstOrDefault();
                    }
                    else if (fieldSchema.FieldType == FieldExactTypes.File)
                    {
                        contentShape[fieldSchema.FieldName] = new FileFieldObject();
                    }
                    else
                    {
                        // TODO: default values for other field types
                        contentShape[fieldSchema.FieldName] = null;
                    }
                }

                VisitChildSchemas(contentSchema, shapesByContentId, FillContentShapes);
            }
        }

        /// <summary>
        /// Удаляем "Id", "ContentId" и "Timestamp" из контентов-расширений
        /// </summary>
        private void FixExtensionContentShapes(Dictionary<int, ContentObject> shapesByContentId)
        {
            foreach (ContentObject contentObject in shapesByContentId.Values)
            {
                foreach (var extObject in contentObject.Values.OfType<ExtensionFieldObject>())
                {
                    foreach (var extContentPair in extObject.Contents.ToArray())
                    {
                        // делаем shallow copy контента, потому что он может использоваться
                        // как нормальный контент в других местах
                        var extContentCopy = new ContentObject();
                        foreach (KeyValuePair<string, object> extContentField in extContentPair.Value)
                        {
                            switch (extContentField.Key)
                            {
                                case ContentObject.IdProp:
                                case ContentObject.ContentIdProp:
                                case ContentObject.TimestampProp:
                                    continue;
                            }
                            extContentCopy[extContentField.Key] = extContentField.Value;
                        }
                        extObject.Contents[extContentPair.Key] = extContentCopy;
                    }
                }
            }
        }

        private void VisitChildSchemas(
            ContentSchema contentSchema,
            Dictionary<int, ContentObject> shapesByContentId,
            Action<ContentSchema, Dictionary<int, ContentObject>> action)
        {
            foreach (FieldSchema fieldSchema in contentSchema.Fields.Values)
            {
                if (fieldSchema is IRelationFieldSchema relationFieldSchema)
                {
                    if (relationFieldSchema.Content is ContentSchema childContentSchema)
                    {
                        action.Invoke(childContentSchema, shapesByContentId);
                    }
                }
                else if (fieldSchema is ExtensionFieldSchema extensionFieldSchema)
                {
                    foreach (var childContentSchema in extensionFieldSchema.Contents.Values.OfType<ContentSchema>())
                    {
                        action.Invoke(childContentSchema, shapesByContentId);
                    }
                }
            }
        }
    }
}
