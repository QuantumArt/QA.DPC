import {
  ContentSchema,
  ExtensionFieldSchema,
  isRelationField,
  isExtensionField
} from "Models/EditorSchemaModels";
import { isPlainObject } from "Utils/TypeChecks";

export interface PartialContentSelection {
  [name: string]: PartialContentSelection | true;
}

export function validateContentSelection(
  contentSchema: ContentSchema,
  selection: PartialContentSelection,
  path: string = ""
) {
  if (process.env.NODE_ENV.toLowerCase() === "production") return;
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
      if (nestedSelection !== true) {
        validateContentSelection(fieldSchema.Content, nestedSelection, fieldPath);
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
  selection: PartialContentSelection,
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
    validateContentSelection(contentSchema, nestedSelection as any, contentPath);
  });
}
