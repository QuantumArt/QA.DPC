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

export class SchemaContext {
  public rootSchema: ContentSchema;

  /**
   * Преобразует JSON Reference ссылки вида `{ "$ref": "#/definitions/MySchema" }`
   * в циклическую структуру объектов. Присоединяет к схемам контентов `.include()`-методы.
   * @param editorSchema Схема редактора
   * @returns Схема корневого контента
   */
  public initSchema(editorSchema: { Content: any; Definitions: { [name: string]: any } }) {
    // editorSchema = JSON.parse(JSON.stringify(editorSchema));
    const definitions = editorSchema.Definitions;
    delete editorSchema.Definitions;
    visitObject(editorSchema, definitions, new Set());
    this.rootSchema = editorSchema.Content;
  }
}

export default new SchemaContext();

function visitObject(object: any, definitions: object, visited: Set<object>): void {
  if (object && typeof object === "object") {
    if (visited.has(object)) {
      return;
    }
    visited.add(object);

    if (Array.isArray(object)) {
      object.forEach((value, index) => {
        object[index] = resolveRef(value, definitions);
        visitObject(object[index], definitions, visited);
      });
    } else {
      Object.keys(object).forEach(key => {
        object[key] = resolveRef(object[key], definitions);
        visitObject(object[key], definitions, visited);
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

function resolveRef(object: any, definitions: object): any {
  return typeof object === "object" && object !== null && "$ref" in object
    ? definitions[object.$ref.slice(14)]
    : object;
}
