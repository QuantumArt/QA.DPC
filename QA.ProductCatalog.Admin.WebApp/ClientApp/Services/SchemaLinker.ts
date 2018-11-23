import { inject } from "react-ioc";
import { extendObservable } from "mobx";
import { DataContext } from "Services/DataContext";
import { DataNormalizer } from "Services/DataNormalizer";
import { DataSerializer } from "Services/DataSerializer";
import {
  ContentSchema,
  isExtensionField,
  isRelationField,
  RelationFieldSchema,
  PreloadingMode,
  PreloadingState,
  isContent,
  isField,
  ContentSchemasById,
  EditorSchema
} from "Models/EditorSchemaModels";
import { TablesSnapshot, EntitySnapshot } from "Models/EditorDataModels";
import { Mutable, isSingleKeyObject, isInteger, isObject } from "Utils/TypeChecks";

export class SchemaLinker {
  @inject private _dataContext: DataContext;
  @inject private _dataNormalizer: DataNormalizer;
  @inject private _dataSerializer: DataSerializer;

  /** Слинковать схему корневого контента */
  public linkNestedSchemas(editorSchema: EditorSchema) {
    const definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;

    linkNestedSchemas(editorSchema, definitions, null);
    return editorSchema.Content;
  }

  /** Слинковать объединенные схемы контентов по Id контента */
  public linkMergedSchemas(mergedSchemas: ContentSchemasById) {
    const mergedSchemasRef = { mergedSchemas };
    linkMergedSchemas(mergedSchemasRef, mergedSchemas, null);
    return mergedSchemasRef.mergedSchemas;
  }

  public addPreloadedArticlesToSnapshot(
    tablesSnapshot: Mutable<TablesSnapshot>,
    contentSchema: ContentSchema
  ) {
    const objectsByContent: {
      [contentName: string]: EntitySnapshot[];
    } = {};

    visitRelationFields(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const objects = objectsByContent[contentName] || (objectsByContent[contentName] = []);
        const articleObjects = this._dataSerializer.deserialize<EntitySnapshot[]>(
          fieldSchema.PreloadedArticles
        );
        objects.push(...articleObjects);
      }
    });

    const preloadedTablesSnapshot = this._dataNormalizer.normalizeTables(objectsByContent);

    Object.entries(preloadedTablesSnapshot).forEach(([contentName, preloadedArticlesById]) => {
      const articlesById = tablesSnapshot[contentName];
      if (articlesById) {
        Object.entries(preloadedArticlesById).forEach(([id, article]) => {
          if (!articlesById[id]) {
            articlesById[id] = article;
          }
        });
      } else {
        tablesSnapshot[contentName] = preloadedArticlesById;
      }
    });
  }

  public linkSchemaWithPreloadedArticles(contentSchema: ContentSchema) {
    visitRelationFields(contentSchema, fieldSchema => {
      if (fieldSchema.PreloadingMode === PreloadingMode.Eager) {
        const contentName = fieldSchema.RelatedContent.ContentName;
        const entitiesMap = this._dataContext.tables[contentName];
        fieldSchema.PreloadingState = PreloadingState.Done;
        fieldSchema.PreloadedArticles = fieldSchema.PreloadedArticles.map(article =>
          entitiesMap.get(String(article._ServerId))
        );
      } else if (fieldSchema.PreloadingMode === PreloadingMode.Lazy) {
        const properties = {
          PreloadingState: PreloadingState.NotStarted
        };
        extendObservable(fieldSchema, properties, null, { deep: false });
      }
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
  lastContent: ContentSchema,
  visited = new Set<Object>()
): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveJsonRef(value, definitions);
        linkNestedSchemas(object[index], definitions, lastContent, visited);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(key => {
        object[key] = resolveJsonRef(object[key], definitions);
        linkNestedSchemas(object[key], definitions, lastContent, visited);
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
  lastContent: ContentSchema,
  visited = new Set<Object>()
): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveContentIdRef(value, mergedSchemas);
        linkMergedSchemas(object[index], mergedSchemas, lastContent, visited);
      });
    } else {
      if (isContent(object)) {
        lastContent = object;
      }
      Object.keys(object).forEach(key => {
        object[key] = resolveContentIdRef(object[key], mergedSchemas);
        linkMergedSchemas(object[key], mergedSchemas, lastContent, visited);
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

function visitRelationFields(
  contentSchema: ContentSchema,
  action: (fieldSchema: RelationFieldSchema) => void,
  visited = new Set<ContentSchema>()
) {
  if (visited.has(contentSchema)) {
    return;
  }
  visited.add(contentSchema);
  Object.values(contentSchema.Fields).forEach(fieldSchema => {
    if (isRelationField(fieldSchema)) {
      action(fieldSchema);
      visitRelationFields(fieldSchema.RelatedContent, action, visited);
    } else if (isExtensionField(fieldSchema)) {
      Object.values(fieldSchema.ExtensionContents).forEach(extensionSchema => {
        visitRelationFields(extensionSchema, action, visited);
      });
    }
  });
}
