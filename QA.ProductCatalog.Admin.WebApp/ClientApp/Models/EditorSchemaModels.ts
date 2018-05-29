import { isObject, isInteger } from "Utils/TypeChecks";

export interface ContentSchema {
  readonly ContentId: number;
  readonly ContentPath: string;
  readonly ContentName: string;
  readonly ContentTitle: string;
  readonly ContentDescription: string;
  readonly ForExtension: boolean;
  readonly Fields: {
    readonly [name: string]: FieldSchema;
  };
  include(selector: (fields: { [name: string]: FieldSchema }) => ProjectionSchema[]): string[];
}

type ProjectionSchema = RelationFieldSchema | BackwardFieldSchema | ExtensionFieldSchema | string[];

export function includeContent(
  this: ContentSchema,
  selector: (fields: { [name: string]: FieldSchema }) => ProjectionSchema[]
): string[] {
  const paths = [this.ContentPath];

  selector(this.Fields).forEach((field, i) => {
    if (isRelationField(field) || isBackwardField(field)) {
      paths.push(field.Content.ContentPath);
    } else if (isExtensionField(field)) {
      const contentsPaths = Object.values(field.Contents).map(content => content.ContentPath);
      paths.push(...contentsPaths);
    } else if (Array.isArray(field) && typeof (field[0] === "string")) {
      paths.push(...field);
    } else {
      throw new TypeError("Invalid field selection [" + i + "]: " + field);
    }
  });

  return paths;
}

export interface FieldSchema {
  readonly FieldId: number;
  readonly FieldName: string;
  readonly FieldTitle: string;
  readonly FieldDescription: string;
  readonly FieldType: string;
  readonly IsRequired: boolean;
  readonly IsReadOnly: boolean;
  readonly DefaultValue: any;
  // TODO: review this
  include(selector: (fields: { [name: string]: FieldSchema }) => ProjectionSchema[]): string[];
  include(selector: (contents: { [name: string]: ContentSchema }) => string[][]): string[];
}

export interface RelationFieldSchema extends FieldSchema {
  readonly IsBackward: false;
  readonly Content: ContentSchema;
}

export interface BackwardFieldSchema extends FieldSchema {
  readonly IsBackward: true;
  readonly Content: ContentSchema;
}

export function includeRelation(
  this: RelationFieldSchema | BackwardFieldSchema,
  selector: (fields: { [name: string]: FieldSchema }) => ProjectionSchema[]
): string[] {
  return includeContent.call(this.Content, selector);
}

export interface ClassifierFieldSchema extends FieldSchema {
  readonly Changeable: boolean;
}

export interface ExtensionFieldSchema extends ClassifierFieldSchema {
  readonly Contents: {
    readonly [name: string]: ContentSchema;
  };
}

export function includeExtension(
  this: ExtensionFieldSchema,
  selector: (contents: { [name: string]: ContentSchema }) => string[][]
): string[] {
  const paths = Object.values(this.Contents).map(content => content.ContentPath);
  selector(this.Contents).forEach((content, i) => {
    if (Array.isArray(content) && typeof (content[0] === "string")) {
      paths.push(...content);
    } else {
      throw new TypeError("Invalid content selection [" + i + "]: " + content);
    }
  });
  return paths;
}

export interface StringFieldSchema extends FieldSchema {
  readonly RegexPattern: string;
}

export interface NumericFieldSchema extends FieldSchema {
  readonly IsInteger: boolean;
}

export interface EnumFieldSchema extends FieldSchema {
  readonly ShowAsRadioButtons: boolean;
  readonly Items: StringEnumItem[];
}

interface StringEnumItem {
  readonly Value: string;
  readonly Alias: string;
  readonly IsDefault: boolean;
  readonly Invalid: boolean;
}

export function isContent(content: any): content is ContentSchema {
  return isObject(content) && isInteger(content.ContentId);
}

export function isField(field: any): field is FieldSchema {
  return isObject(field) && isInteger(field.FieldId);
}

export function isRelationField(field: any): field is RelationFieldSchema {
  return (
    isObject(field) &&
    isInteger(field.FieldId) &&
    field.FieldType.endsWith("Relation") &&
    isContent(field.Content) &&
    !field.IsBackward
  );
}

export function isBackwardField(field: any): field is BackwardFieldSchema {
  return (
    isObject(field) &&
    isInteger(field.FieldId) &&
    field.FieldType.endsWith("Relation") &&
    isContent(field.Content) &&
    field.IsBackward
  );
}

export function isClassifierField(field: any): field is ClassifierFieldSchema {
  return isObject(field) && isInteger(field.FieldId) && field.FieldType === "Classifier";
}

export function isExtensionField(field: any): field is ExtensionFieldSchema {
  return (
    isObject(field) &&
    isInteger(field.FieldId) &&
    field.FieldType === "Classifier" &&
    isObject(field.Contents) &&
    Object.values(field.Contents).every(isContent)
  );
}

export function isStringField(field: any): field is StringFieldSchema {
  return isObject(field) && isInteger(field.FieldId) && field.FieldType === "String";
}

export function isNumericField(field: any): field is EnumFieldSchema {
  return isObject(field) && isInteger(field.FieldId) && field.FieldType === "Numeric";
}

export function isEnumField(field: any): field is EnumFieldSchema {
  return isObject(field) && isInteger(field.FieldId) && field.FieldType === "StringEnum";
}
