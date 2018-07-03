import {
  ContentSchema,
  ExtensionFieldSchema,
  isRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";
import { isPlainObject } from "Utils/TypeChecks";

export interface RelationSelection {
  [name: string]: RelationSelection | true | null;
}

export function validateRelationSelection(
  contentSchema: ContentSchema,
  selection: RelationSelection,
  path: string = ""
) {
  if (!isPlainObject(selection)) {
    throw new Error(`Content selection ${path || "/"} should be an object.`);
  }
  if (Object.keys(selection).length === 0) {
    throw new Error(`Content selection ${path || "/"} should cover some fields.`);
  }
  Object.entries(selection).forEach(([fieldName, nestedSelection]) => {
    const fieldPath = `${path}/${fieldName}`;
    if (!(fieldName in contentSchema.Fields)) {
      throw new Error(`Content selection field ${fieldPath} was not found in ContentSchema.`);
    }
    const fieldSchema = contentSchema.Fields[fieldName];
    if (isRelationField(fieldSchema)) {
      if (nestedSelection !== true && nestedSelection !== null) {
        validateRelationSelection(fieldSchema.Content, nestedSelection, fieldPath);
      }
    } else if (isExtensionField(fieldSchema)) {
      validateExtensionSelection(fieldSchema, nestedSelection as any, fieldPath);
    } else {
      throw new Error(
        `Content selection field ${fieldPath} should be Extension or Relation. Got FieldExactTypes.${
          fieldSchema.FieldType
        } instead.`
      );
    }
  });
}

function validateExtensionSelection(
  fieldSchema: ExtensionFieldSchema,
  selection: RelationSelection,
  path: string
) {
  if (!isPlainObject(selection)) {
    throw new Error(`Extension selection ${path} should be an object.`);
  }
  if (Object.keys(selection).length === 0) {
    throw new Error(`Extension selection ${path} should cover some contents.`);
  }
  Object.entries(selection).forEach(([contentName, nestedSelection]) => {
    const contentPath = `${path}/${contentName}`;
    if (!(contentName in fieldSchema.Contents)) {
      throw new Error(`Extension selection content ${contentPath} was not found in FieldSchema.`);
    }
    const contentSchema = fieldSchema.Contents[contentName];
    validateRelationSelection(contentSchema, nestedSelection as any, contentPath);
  });
}
