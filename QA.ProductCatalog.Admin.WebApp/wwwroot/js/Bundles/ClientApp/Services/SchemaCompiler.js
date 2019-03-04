import {
  isRelationField,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  UpdatingMode
} from "Models/EditorSchemaModels";
import { ComputedCache } from "Utils/WeakCache";
var SchemaCompiler = /** @class */ (function() {
  function SchemaCompiler() {}
  SchemaCompiler.prototype.compileSchemaFunctions = function(contentSchema) {
    visitContentSchema(contentSchema, function(contentSchema) {
      var isEditedCache = new ComputedCache();
      var isEdited = compileRecursivePredicate(contentSchema, "isEdited");
      contentSchema.isEdited = function(article) {
        return isEditedCache.getOrAdd(article, function() {
          return isEdited(contentSchema, article);
        });
      };
      var isTouchedCache = new ComputedCache();
      var isTouched = compileRecursivePredicate(contentSchema, "isTouched");
      contentSchema.isTouched = function(article) {
        return isTouchedCache.getOrAdd(article, function() {
          return isTouched(contentSchema, article);
        });
      };
      var isChangedCache = new ComputedCache();
      var isChanged = compileRecursivePredicate(contentSchema, "isChanged");
      contentSchema.isChanged = function(article) {
        return isChangedCache.getOrAdd(article, function() {
          return isChanged(contentSchema, article);
        });
      };
      var hasErrorsCache = new ComputedCache();
      var hasErrors = compileRecursivePredicate(contentSchema, "hasErrors");
      contentSchema.hasErrors = function(article) {
        return hasErrorsCache.getOrAdd(article, function() {
          return hasErrors(contentSchema, article);
        });
      };
      var hasVisibleErrorsCache = new ComputedCache();
      var hasVisibleErrors = compileRecursivePredicate(
        contentSchema,
        "hasVisibleErrors"
      );
      contentSchema.hasVisibleErrors = function(article) {
        return hasVisibleErrorsCache.getOrAdd(article, function() {
          return hasVisibleErrors(contentSchema, article);
        });
      };
      var getLastModifiedCache = new ComputedCache();
      var getLastModified = compileRecursiveGetLastModified(contentSchema);
      contentSchema.getLastModified = function(article) {
        return getLastModifiedCache.getOrAdd(article, function() {
          return getLastModified(contentSchema, article);
        });
      };
    });
  };
  return SchemaCompiler;
})();
export { SchemaCompiler };
function visitContentSchema(contentSchema, action, visited) {
  if (visited === void 0) {
    visited = new Set();
  }
  if (visited.has(contentSchema)) {
    return;
  }
  visited.add(contentSchema);
  action(contentSchema);
  Object.values(contentSchema.Fields).forEach(function(fieldSchema) {
    if (isRelationField(fieldSchema)) {
      visitContentSchema(fieldSchema.RelatedContent, action, visited);
    } else if (isExtensionField(fieldSchema)) {
      Object.values(fieldSchema.ExtensionContents).forEach(function(
        extensionSchema
      ) {
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
function compileRecursivePredicate(contentSchema, funcName) {
  var complexFields = Object.values(contentSchema.Fields).filter(function(
    fieldSchema
  ) {
    return (
      (isRelationField(fieldSchema) &&
        fieldSchema.UpdatingMode === UpdatingMode.Update) ||
      isExtensionField(fieldSchema)
    );
  });
  return Function(
    "contentSchema",
    "article",
    "\n    var fields = contentSchema.Fields;\n    return article." +
      funcName +
      "()\n    " +
      complexFields
        .map(function(fieldSchema) {
          var name = fieldSchema.FieldName;
          if (isSingleRelationField(fieldSchema)) {
            return (
              " ||\n            article." +
              name +
              " && fields." +
              name +
              ".RelatedContent." +
              funcName +
              "(article." +
              name +
              ")"
            );
          }
          if (isMultiRelationField(fieldSchema)) {
            return (
              " ||\n            article." +
              name +
              " && article." +
              name +
              ".some(function(item) {\n              return fields." +
              name +
              ".RelatedContent." +
              funcName +
              "(item);\n            })"
            );
          }
          return (
            " ||\n            article." +
            name +
            " && fields." +
            name +
            ".ExtensionContents[article." +
            name +
            "]\n              ." +
            funcName +
            "(article." +
            name +
            "_Extension[article." +
            name +
            "])"
          );
        })
        .join("") +
      ";"
  );
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
function compileRecursiveGetLastModified(contentSchema) {
  var complexFields = Object.values(contentSchema.Fields).filter(function(
    fieldSchema
  ) {
    return (
      (isRelationField(fieldSchema) &&
        fieldSchema.UpdatingMode === UpdatingMode.Update) ||
      isExtensionField(fieldSchema)
    );
  });
  return new Function(
    "contentSchema",
    "article",
    "\n    var lastModified = article._Modified;\n    " +
      complexFields
        .map(function(fieldSchema) {
          var name = fieldSchema.FieldName;
          if (isSingleRelationField(fieldSchema)) {
            return (
              "\n          if (article." +
              name +
              ") {\n            var relatedContent = contentSchema.Fields." +
              name +
              ".RelatedContent;\n            var relationModified = relatedContent.getLastModified(article." +
              name +
              ");\n            if (lastModified < relationModified) {\n              lastModified = relationModified;\n            }\n          }"
            );
          }
          if (isMultiRelationField(fieldSchema)) {
            return (
              "\n          if (article." +
              name +
              ") {\n            var relatedContent = contentSchema.Fields." +
              name +
              ".RelatedContent;\n            article." +
              name +
              ".forEach(function(item) {\n              var itemModified = relatedContent.getLastModified(item);\n              if (lastModified < itemModified) {\n                lastModified = itemModified;\n              }\n            });\n          }"
            );
          }
          return (
            "\n        if (article." +
            name +
            ") {\n          var extensionContent = contentSchema.Fields." +
            name +
            ".ExtensionContents[article." +
            name +
            "];\n          var extensionModified = extensionContent.getLastModified(article." +
            name +
            "_Extension[article." +
            name +
            "]);\n          if (lastModified < extensionModified) {\n            lastModified = extensionModified;\n          }\n        }"
          );
        })
        .join("") +
      "\n    return lastModified;"
  );
}
//# sourceMappingURL=SchemaCompiler.js.map
