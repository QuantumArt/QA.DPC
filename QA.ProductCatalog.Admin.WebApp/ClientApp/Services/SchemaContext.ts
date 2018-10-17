import {
  ContentSchema,
  EditorSchema,
  ContentSchemasById,
  isContent,
  isField,
  isRelationField,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  UpdatingMode
} from "Models/EditorSchemaModels";
import { isObject, isInteger, isSingleKeyObject } from "Utils/TypeChecks";
import { ArticleObject } from "Models/EditorDataModels";
import { ComputedCache } from "Utils/WeakCache";

export class SchemaContext {
  /** Схема корневого контента */
  public rootSchema: ContentSchema;
  /** Объединенные схемы контентов по Id контента */
  public contentSchemasById: ContentSchemasById;
  /** Объединенные схемы контентов по имени контента */
  public contentSchemasByName: {
    readonly [contentName: string]: ContentSchema;
  };

  public initSchema(editorSchema: EditorSchema, mergedSchemas: ContentSchemasById) {
    const definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;

    linkNestedSchemas(editorSchema, definitions, new Set(), null);
    this.rootSchema = editorSchema.Content;

    const mergedSchemasRef = { mergedSchemas };
    linkMergedSchemas(mergedSchemasRef, mergedSchemas, new Set(), null);
    this.contentSchemasById = mergedSchemasRef.mergedSchemas;

    this.contentSchemasByName = Object.values(mergedSchemas).reduce((obj, contentSchema) => {
      obj[contentSchema.ContentName] = contentSchema;
      return obj;
    }, {});

    visitContentSchema(this.rootSchema, contentSchema => {
      const isEditedCache = new ComputedCache();
      const isEdited = compileRecursivePredicate(contentSchema, "isEdited");
      contentSchema.isEdited = (article: ArticleObject) =>
        isEditedCache.getOrAdd(article, () => isEdited(contentSchema, article));

      const isTouchedCache = new ComputedCache();
      const isTouched = compileRecursivePredicate(contentSchema, "isTouched");
      contentSchema.isTouched = (article: ArticleObject) =>
        isTouchedCache.getOrAdd(article, () => isTouched(contentSchema, article));

      const isChangedCache = new ComputedCache();
      const isChanged = compileRecursivePredicate(contentSchema, "isChanged");
      contentSchema.isChanged = (article: ArticleObject) =>
        isChangedCache.getOrAdd(article, () => isChanged(contentSchema, article));

      const hasErrorsCache = new ComputedCache();
      const hasErrors = compileRecursivePredicate(contentSchema, "hasErrors");
      contentSchema.hasErrors = (article: ArticleObject) =>
        hasErrorsCache.getOrAdd(article, () => hasErrors(contentSchema, article));

      const hasVisibleErrorsCache = new ComputedCache();
      const hasVisibleErrors = compileRecursivePredicate(contentSchema, "hasVisibleErrors");
      contentSchema.hasVisibleErrors = (article: ArticleObject) =>
        hasVisibleErrorsCache.getOrAdd(article, () => hasVisibleErrors(contentSchema, article));

      const lastModifiedCache = new ComputedCache();
      const lastModified = compileRecursiveLastModified(contentSchema);
      contentSchema.lastModified = (article: ArticleObject) =>
        lastModifiedCache.getOrAdd(article, () => lastModified(contentSchema, article));
    });
  }
}

/**
 * Преобразует JSON Reference ссылки на `ContentSchema` вида
 * `{ "$ref": "#/definitions/MySchema2" }` в циклическую структуру объектов.
 * Заполняет обратные ссылки `FieldSchema.ParentContent`.
 */
function linkNestedSchemas(
  object: any,
  definitions: { [name: string]: any },
  visited: Set<Object>,
  lastContent: ContentSchema
): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveJsonRef(value, definitions);
        linkNestedSchemas(object[index], definitions, visited, lastContent);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(key => {
        object[key] = resolveJsonRef(object[key], definitions);
        linkNestedSchemas(object[key], definitions, visited, lastContent);
      });
      if (isField(object)) {
        object.ParentContent = lastContent;
      }
    }
  }
}

function resolveJsonRef(object: any, definitions: { [name: string]: any }): any {
  return isObject(object) && "$ref" in object ? definitions[object.$ref.slice(14)] : object;
}

/**
 * Преобразует ссылки на `ContentSchema` вида `{ "ContentId": 1234 }`
 * в циклическую структуру объектов. Заполняет обратные ссылки `FieldSchema.ParentContent`.
 */
function linkMergedSchemas(
  object: any,
  mergedSchemas: { [name: string]: any },
  visited: Set<Object>,
  lastContent: ContentSchema
): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveContentIdRef(value, mergedSchemas);
        linkMergedSchemas(object[index], mergedSchemas, visited, lastContent);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(key => {
        object[key] = resolveContentIdRef(object[key], mergedSchemas);
        linkMergedSchemas(object[key], mergedSchemas, visited, lastContent);
      });
      if (isField(object)) {
        object.ParentContent = lastContent;
      }
    }
  }
}

function resolveContentIdRef(object: any, mergedSchemas: ContentSchemasById) {
  return isSingleKeyObject(object) && isInteger(object.ContentId)
    ? mergedSchemas[object.ContentId]
    : object;
}

function visitContentSchema(
  contentSchema: ContentSchema,
  action: (contentSchema: ContentSchema) => void,
  visited = new Set<ContentSchema>()
) {
  if (visited.has(contentSchema)) {
    return;
  }
  visited.add(contentSchema);
  action(contentSchema);

  Object.values(contentSchema.Fields).forEach(fieldSchema => {
    if (isRelationField(fieldSchema)) {
      visitContentSchema(fieldSchema.RelatedContent, action, visited);
    } else if (isExtensionField(fieldSchema)) {
      Object.values(fieldSchema.ExtensionContents).forEach(extensionSchema => {
        visitContentSchema(extensionSchema, action, visited);
      });
    }
  });
}

/**
 * Компилирует булевскую функцию для интерфейса `ContentSchema` на основе обхода полей схемы
 * @example
 * function isChanged(contentSchema, article) {
 *   var fields = contentSchema.Fields;
 *   return (
 *     article.isChanged() ||
 *     (article.Type &&
 *       fields.Type.ExtensionContents[article.Type].isChanged(
 *         article.Type_Extension[article.Type]
 *       )) ||
 *     (article.Parameters &&
 *       article.Parameters.some(function(item) {
 *         return fields.Parameters.RelatedContent.isChanged(item);
 *       }))
 *   );
 * }
 */
function compileRecursivePredicate(
  contentSchema: ContentSchema,
  funcName: string
): (contentSchema: ContentSchema, article: ArticleObject) => boolean {
  const complexFields = Object.values(contentSchema.Fields).filter(
    fieldSchema =>
      (isRelationField(fieldSchema) && fieldSchema.UpdatingMode === UpdatingMode.Update) ||
      isExtensionField(fieldSchema)
  );
  return Function(
    "contentSchema",
    "article",
    `
    var fields = contentSchema.Fields;
    return article.${funcName}()
    ${complexFields
      .map(fieldSchema => {
        const name = fieldSchema.FieldName;
        if (isSingleRelationField(fieldSchema)) {
          return ` ||
            article.${name} && fields.${name}.RelatedContent.${funcName}(article.${name})`;
        }
        if (isMultiRelationField(fieldSchema)) {
          return ` ||
            article.${name} && article.${name}.some(function(item) {
              return fields.${name}.RelatedContent.${funcName}(item);
            })`;
        }
        return ` ||
            article.${name} && fields.${name}.ExtensionContents[article.${name}]
              .${funcName}(article.${name}_Extension[article.${name}])`;
      })
      .join("")};`
  ) as any;
}

/**
 * Компилирует функцию для интерфейса `ContentSchema`, возвращающую максимальную
 * дату модификации по всем статьям, входящим в продукт, на основе обхода полей схемы.
 * @example
 * function lastModified(contentSchema, article) {
 *   var lastModified = article._Modified;
 *   if (article.Type) {
 *     var extensionContent = contentSchema.Fields.Type.ExtensionContents[article.Type];
 *     var extensionModified = extensionContent.lastModified(article.Type_Extension[article.Type]);
 *     if (lastModified < extensionModified) {
 *       lastModified = extensionModified;
 *     }
 *   }
 *   if (article.Parameters) {
 *     var relatedContent = contentSchema.Fields.Parameters.RelatedContent;
 *     article.Parameters.forEach(function(item) {
 *       var itemModified = relatedContent.lastModified(item);
 *       if (lastModified < itemModified) {
 *         lastModified = itemModified;
 *       }
 *     });
 *   }
 *   return lastModified;
 * }
 */
function compileRecursiveLastModified(
  contentSchema: ContentSchema
): (contentSchema: ContentSchema, article: ArticleObject) => Date {
  const complexFields = Object.values(contentSchema.Fields).filter(
    fieldSchema =>
      (isRelationField(fieldSchema) && fieldSchema.UpdatingMode === UpdatingMode.Update) ||
      isExtensionField(fieldSchema)
  );
  return new Function(
    "contentSchema",
    "article",
    `
    var lastModified = article._Modified;
    ${complexFields
      .map(fieldSchema => {
        const name = fieldSchema.FieldName;
        if (isSingleRelationField(fieldSchema)) {
          return `
          if (article.${name}) {
            var relatedContent = contentSchema.Fields.${name}.RelatedContent;
            var relationModified = relatedContent.lastModified(article.${name});
            if (lastModified < relationModified) {
              lastModified = relationModified;
            }
          }`;
        }
        if (isMultiRelationField(fieldSchema)) {
          return `
          if (article.${name}) {
            var relatedContent = contentSchema.Fields.${name}.RelatedContent;
            article.${name}.forEach(function(item) {
              var itemModified = relatedContent.lastModified(item);
              if (lastModified < itemModified) {
                lastModified = itemModified;
              }
            });
          }`;
        }
        return `
        if (article.${name}) {
          var extensionContent = contentSchema.Fields.${name}.ExtensionContents[article.${name}];
          var extensionModified = extensionContent.lastModified(article.${name}_Extension[article.${name}]);
          if (lastModified < extensionModified) {
            lastModified = extensionModified;
          }
        }`;
      })
      .join("")}
    return lastModified;`
  ) as any;
}
