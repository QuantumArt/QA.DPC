import * as tslib_1 from "tslib";
import { inject } from "react-ioc";
import { extendObservable } from "mobx";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import {
  isExtensionField,
  isRelationField,
  PreloadingMode,
  PreloadingState,
  isContent,
  isField
} from "Models/EditorSchemaModels";
import { isSingleKeyObject, isInteger, isObject } from "Utils/TypeChecks";
var SchemaLinker = /** @class */ (function() {
  function SchemaLinker() {}
  /** Слинковать схему корневого контента */
  SchemaLinker.prototype.linkNestedSchemas = function(editorSchema) {
    var definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;
    linkNestedSchemas(editorSchema, definitions, null);
    return editorSchema.Content;
  };
  /** Слинковать объединенные схемы контентов по Id контента */
  SchemaLinker.prototype.linkMergedSchemas = function(mergedSchemas) {
    var mergedSchemasRef = { mergedSchemas: mergedSchemas };
    linkMergedSchemas(mergedSchemasRef, mergedSchemas, null);
    return mergedSchemasRef.mergedSchemas;
  };
  SchemaLinker.prototype.addPreloadedArticlesToSnapshot = function(
    tablesSnapshot,
    contentSchema
  ) {
    var _this = this;
    var objectsByContent = {};
    visitRelationFields(contentSchema, function(fieldSchema) {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        var contentName = fieldSchema.RelatedContent.ContentName;
        var objects =
          objectsByContent[contentName] || (objectsByContent[contentName] = []);
        var articleObjects = _this._dataSerializer.deserialize(
          fieldSchema.PreloadedArticles
        );
        objects.push.apply(objects, tslib_1.__spread(articleObjects));
      }
    });
    var preloadedTablesSnapshot = this._dataNormalizer.normalizeTables(
      objectsByContent
    );
    Object.entries(preloadedTablesSnapshot).forEach(function(_a) {
      var _b = tslib_1.__read(_a, 2),
        contentName = _b[0],
        preloadedArticlesById = _b[1];
      var articlesById = tablesSnapshot[contentName];
      if (articlesById) {
        Object.entries(preloadedArticlesById).forEach(function(_a) {
          var _b = tslib_1.__read(_a, 2),
            id = _b[0],
            article = _b[1];
          if (!articlesById[id]) {
            articlesById[id] = article;
          }
        });
      } else {
        tablesSnapshot[contentName] = preloadedArticlesById;
      }
    });
  };
  SchemaLinker.prototype.linkSchemaWithPreloadedArticles = function(
    contentSchema
  ) {
    var _this = this;
    visitRelationFields(contentSchema, function(fieldSchema) {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        var contentName = fieldSchema.RelatedContent.ContentName;
        var entitiesMap_1 = _this._dataContext.tables[contentName];
        fieldSchema.PreloadingState = PreloadingState.Done;
        fieldSchema.PreloadedArticles = fieldSchema.PreloadedArticles.map(
          function(article) {
            return entitiesMap_1.get(String(article._ServerId));
          }
        );
      } else if (fieldSchema.PreloadingMode === PreloadingMode.Lazy) {
        var properties = {
          PreloadingState: PreloadingState.NotStarted
        };
        extendObservable(fieldSchema, properties, null, { deep: false });
      }
    });
  };
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataContext)],
    SchemaLinker.prototype,
    "_dataContext",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataNormalizer)],
    SchemaLinker.prototype,
    "_dataNormalizer",
    void 0
  );
  tslib_1.__decorate(
    [inject, tslib_1.__metadata("design:type", DataSerializer)],
    SchemaLinker.prototype,
    "_dataSerializer",
    void 0
  );
  return SchemaLinker;
})();
export { SchemaLinker };
/**
 * Преобразует JSON Reference ссылки на `ContentSchema` вида
 * `{ "$ref": "#/definitions/MySchema2" }` в циклическую структуру объектов.
 * Заполняет обратные ссылки `FieldSchema.ParentContent`.
 */
function linkNestedSchemas(object, definitions, lastContent, visited) {
  if (visited === void 0) {
    visited = new Set();
  }
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);
    if (Array.isArray(object)) {
      object.forEach(function(value, index) {
        object[index] = resolveJsonRef(value, definitions);
        linkNestedSchemas(object[index], definitions, lastContent, visited);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(function(key) {
        object[key] = resolveJsonRef(object[key], definitions);
        linkNestedSchemas(object[key], definitions, lastContent, visited);
      });
      if (isField(object)) {
        object.ParentContent = lastContent;
      }
    }
  }
}
function resolveJsonRef(object, definitions) {
  return isObject(object) && "$ref" in object
    ? definitions[object.$ref.slice(14)]
    : object;
}
/**
 * Преобразует ссылки на `ContentSchema` вида `{ "ContentId": 1234 }`
 * в циклическую структуру объектов. Заполняет обратные ссылки `FieldSchema.ParentContent`.
 */
function linkMergedSchemas(object, mergedSchemas, lastContent, visited) {
  if (visited === void 0) {
    visited = new Set();
  }
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);
    if (Array.isArray(object)) {
      object.forEach(function(value, index) {
        object[index] = resolveContentIdRef(value, mergedSchemas);
        linkMergedSchemas(object[index], mergedSchemas, lastContent, visited);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(function(key) {
        object[key] = resolveContentIdRef(object[key], mergedSchemas);
        linkMergedSchemas(object[key], mergedSchemas, lastContent, visited);
      });
      if (isField(object)) {
        object.ParentContent = lastContent;
      }
    }
  }
}
function resolveContentIdRef(object, mergedSchemas) {
  return isSingleKeyObject(object) && isInteger(object.ContentId)
    ? mergedSchemas[object.ContentId]
    : object;
}
function visitRelationFields(contentSchema, action, visited) {
  if (visited === void 0) {
    visited = new Set();
  }
  if (visited.has(contentSchema)) {
    return;
  }
  visited.add(contentSchema);
  Object.values(contentSchema.Fields).forEach(function(fieldSchema) {
    if (isRelationField(fieldSchema)) {
      action(fieldSchema);
      visitRelationFields(fieldSchema.RelatedContent, action, visited);
    } else if (isExtensionField(fieldSchema)) {
      Object.values(fieldSchema.ExtensionContents).forEach(function(
        extensionSchema
      ) {
        visitRelationFields(extensionSchema, action, visited);
      });
    }
  });
}
//# sourceMappingURL=SchemaLinker.js.map
