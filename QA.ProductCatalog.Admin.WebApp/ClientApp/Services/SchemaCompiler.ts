import {
  ContentSchema,
  isRelationField,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  UpdatingMode
} from "Models/EditorSchemaModels";
import { ArticleObject } from "Models/EditorDataModels";
import { ComputedCache } from "Utils/WeakCache";

export class SchemaCompiler {
  public compileSchemaFunctions(contentSchema: ContentSchema) {
    visitContentSchema(contentSchema, contentSchema => {
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

      const getLastModifiedCache = new ComputedCache();
      const getLastModified = compileRecursiveGetLastModified(contentSchema);
      contentSchema.getLastModified = (article: ArticleObject) =>
        getLastModifiedCache.getOrAdd(article, () => getLastModified(contentSchema, article));
    });
  }
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
 * function getLastModified(contentSchema, article) {
 *   var lastModified = article._Modified;
 *   if (article.Type) {
 *     var extensionContent = contentSchema.Fields.Type.ExtensionContents[article.Type];
 *     var extensionModified = extensionContent.getLastModified(article.Type_Extension[article.Type]);
 *     if (lastModified < extensionModified) {
 *       lastModified = extensionModified;
 *     }
 *   }
 *   if (article.Parameters) {
 *     var relatedContent = contentSchema.Fields.Parameters.RelatedContent;
 *     article.Parameters.forEach(function(item) {
 *       var itemModified = relatedContent.getLastModified(item);
 *       if (lastModified < itemModified) {
 *         lastModified = itemModified;
 *       }
 *     });
 *   }
 *   return lastModified;
 * }
 */
function compileRecursiveGetLastModified(
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
            var relationModified = relatedContent.getLastModified(article.${name});
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
              var itemModified = relatedContent.getLastModified(item);
              if (lastModified < itemModified) {
                lastModified = itemModified;
              }
            });
          }`;
        }
        return `
        if (article.${name}) {
          var extensionContent = contentSchema.Fields.${name}.ExtensionContents[article.${name}];
          var extensionModified = extensionContent.getLastModified(article.${name}_Extension[article.${name}]);
          if (lastModified < extensionModified) {
            lastModified = extensionModified;
          }
        }`;
      })
      .join("")}
    return lastModified;`
  ) as any;
}
