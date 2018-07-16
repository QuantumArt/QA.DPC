import { ContentSchema } from "Models/EditorSchemaModels";
import { isObject } from "Utils/TypeChecks";

export class SchemaContext {
  public contentSchema: ContentSchema;
  public contentSchemasById: {
    readonly [contentId: number]: ContentSchema;
  };

  public initSchema(editorSchema: { Content: any; Definitions: { [name: string]: any } }) {
    // editorSchema = JSON.parse(JSON.stringify(editorSchema));
    const definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;
    visitSchema(editorSchema, definitions, new Set());
    this.contentSchema = editorSchema.Content;
    this.contentSchemasById = definitions;
  }
}

/**
 * Преобразует JSON Reference ссылки вида `{ "$ref": "#/definitions/MySchema" }`
 * в циклическую структуру объектов.
 */
function visitSchema(
  object: any,
  definitions: { [name: string]: any },
  visited: Set<Object>
): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveRef(value, definitions);
        visitSchema(object[index], definitions, visited);
      });
    } else {
      Object.keys(object).forEach(key => {
        object[key] = resolveRef(object[key], definitions);
        visitSchema(object[key], definitions, visited);
      });
    }
  }
}

function resolveRef(object: any, definitions: { [name: string]: any }): any {
  return isObject(object) && "$ref" in object ? definitions[object.$ref.slice(14)] : object;
}
