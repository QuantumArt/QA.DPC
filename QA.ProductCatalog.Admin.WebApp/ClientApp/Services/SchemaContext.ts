import {
  ContentSchema,
  EditorSchema,
  ContentSchemasById,
  isContent,
  isField
} from "Models/EditorSchemaModels";
import { isObject, isInteger, isSingleKeyObject } from "Utils/TypeChecks";

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
        // @ts-ignore
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
        // @ts-ignore
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
