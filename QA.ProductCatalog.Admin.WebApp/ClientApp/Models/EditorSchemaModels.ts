import { isObject, isInteger } from "Utils/TypeChecks";

export interface ContentSchema {
  ContentId: number;
  ContentPath: string;
  ContentName: string;
  ContentTitle: string;
  ContentDescription: string;
  ForExtension: boolean;
  Fields: { [name: string]: FieldSchema };
}

export interface FieldSchema {
  FieldId: number;
  FieldName: string;
  FieldTitle: string;
  FieldDescription: string;
  FieldType: string;
  IsRequired: boolean;
}

export interface RelationFieldSchema extends FieldSchema {
  IsBackward: false;
  Content: ContentSchema;
}

export interface BackwardFieldSchema extends FieldSchema {
  IsBackward: true;
  Content: ContentSchema;
}

export interface ExtensionFieldSchema extends FieldSchema {
  Contents: { [name: string]: ContentSchema };
}

export interface EnumFieldSchema extends FieldSchema {
  Items: StringEnumItem[];
}

interface StringEnumItem {
  Value: string;
  Alias: string;
  IsDefault: boolean;
  Invalid: boolean;
}

export function isRelationField(field: any): field is RelationFieldSchema {
  return (
    isObject(field) &&
    field.FieldType.endsWith("Relation") &&
    isContentRef(field.Content) &&
    !field.IsBackward
  );
}

export function isBackwardField(field: any): field is BackwardFieldSchema {
  return (
    isObject(field) &&
    field.FieldType.endsWith("Relation") &&
    isContentRef(field.Content) &&
    field.IsBackward
  );
}

export function isExtensionField(field: any): field is ExtensionFieldSchema {
  return (
    isObject(field) &&
    field.FieldType === "Classifier" &&
    isObject(field.Contents) &&
    Object.values(field.Contents).every(isContentRef)
  );
}

export function isEnumField(field: any): field is EnumFieldSchema {
  return isObject(field) && field.FieldType === "StringEnum";
}

function isContentRef(content: ContentSchema): boolean {
  return isObject(content) && Object.keys(content).length === 1 && isInteger(content.ContentId);
}
