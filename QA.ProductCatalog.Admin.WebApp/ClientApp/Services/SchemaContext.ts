import {
  ContentSchema,
  includeContent,
  includeRelation,
  includeExtension,
  isContent,
  isField,
  isRelationField,
  isBackwardField,
  isExtensionField
} from "Models/EditorSchemaModels";
import { isObject } from "Utils/TypeChecks";

export class SchemaContext {
  public rootSchema: ContentSchema;

  public initSchema(editorSchema: { Content: any; Definitions: { [name: string]: any } }) {
    // editorSchema = JSON.parse(JSON.stringify(editorSchema));
    const definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;
    visitSchema(editorSchema, definitions, new Set());
    this.rootSchema = editorSchema.Content;
  }
}

export default new SchemaContext();

/**
 * Преобразует JSON Reference ссылки вида `{ "$ref": "#/definitions/MySchema" }`
 * в циклическую структуру объектов. Присоединяет к схемам контентов `.include()`-методы.
 */
function visitSchema(
  object: any,
  definitions: { [name: string]: any },
  visited: Set<object>
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
      if (isContent(object)) {
        object.include = includeContent;
      } else if (isField(object)) {
        if (isRelationField(object) || isBackwardField(object)) {
          // TODO: review this
          object.include = includeRelation as any;
        } else if (isExtensionField(object)) {
          // TODO: review this
          object.include = includeExtension as any;
        }
      }
    }
  }
}

function resolveRef(object: any, definitions: { [name: string]: any }): any {
  return isObject(object) && "$ref" in object ? definitions[object.$ref.slice(14)] : object;
}
