import { isObject, isInteger } from "Utils/TypeChecks";
import { EntityObject } from "Models/EditorDataModels";

export interface ContentSchema {
  readonly ContentId: number;
  readonly ContentPath: string;
  readonly ContentName: string;
  readonly ContentTitle?: string;
  readonly ContentDescription?: string;
  readonly DisplayFieldName?: string;
  /** Используется только в качестве расширения */
  readonly ForExtension: boolean;
  readonly Fields: {
    readonly [name: string]: FieldSchema;
  };
}

export function isContent(content: any): content is ContentSchema {
  return isObject(content) && isInteger(content.ContentId);
}

export interface FieldSchema {
  readonly FieldId: number;
  readonly FieldOrder: number;
  readonly FieldName: string;
  readonly FieldTitle?: string;
  readonly FieldDescription?: string;
  readonly FieldType: FieldExactTypes;
  readonly IsRequired: boolean;
  readonly IsReadOnly: boolean;
  readonly ViewInList: boolean;
  readonly DefaultValue?: any;
  readonly ClassNames: string[];
  readonly ParentContent: ContentSchema;
}

export function isField(field: any): field is FieldSchema {
  return isObject(field) && isInteger(field.FieldId);
}

export enum FieldExactTypes {
  Undefined = "Undefined",
  String = "String",
  Numeric = "Numeric",
  Boolean = "Boolean",
  Date = "Date",
  Time = "Time",
  DateTime = "DateTime",
  File = "File",
  Image = "Image",
  Textbox = "Textbox",
  VisualEdit = "VisualEdit",
  DynamicImage = "DynamicImage",
  M2ORelation = "M2ORelation",
  O2MRelation = "O2MRelation",
  M2MRelation = "M2MRelation",
  Classifier = "Classifier",
  StringEnum = "StringEnum"
}

export interface PlainFieldSchema extends FieldSchema {}

export function isPlainField(field: any): field is PlainFieldSchema {
  return isField(field) && field.ClassNames.includes("PlainFieldSchema");
}

export interface StringFieldSchema extends PlainFieldSchema {
  readonly RegexPattern?: string;
}

export function isStringField(field: any): field is StringFieldSchema {
  return isPlainField(field) && field.ClassNames.includes("StringFieldSchema");
}

export interface NumericFieldSchema extends PlainFieldSchema {
  readonly IsInteger: boolean;
}

export function isNumericField(field: any): field is NumericFieldSchema {
  return isPlainField(field) && field.ClassNames.includes("NumericFieldSchema");
}

export interface ClassifierFieldSchema extends PlainFieldSchema {
  readonly Changeable: boolean;
}

export function isClassifierField(field: any): field is ClassifierFieldSchema {
  return isPlainField(field) && field.ClassNames.includes("ClassifierFieldSchema");
}

export interface FileFieldSchema extends PlainFieldSchema {
  readonly UseSiteLibrary: boolean;
  readonly SubFolder: string;
  readonly LibraryEntityId: number;
  readonly LibraryParentEntityId: number;
}

export function isFileField(field: any): field is FileFieldSchema {
  return isPlainField(field) && field.ClassNames.includes("FileFieldSchema");
}

export interface EnumFieldSchema extends PlainFieldSchema {
  readonly ShowAsRadioButtons: boolean;
  readonly Items: StringEnumItem[];
}

export function isEnumField(field: any): field is EnumFieldSchema {
  return isPlainField(field) && field.ClassNames.includes("EnumFieldSchema");
}

interface StringEnumItem {
  readonly Value: string;
  readonly Alias?: string;
  readonly IsDefault: boolean;
  readonly Invalid: boolean;
}

export interface RelationFieldSchema extends FieldSchema {
  readonly RelatedContent: ContentSchema;
  readonly CloningMode: CloningMode;
  readonly UpdatingMode: UpdatingMode;
  readonly IsDpcBackwardField: boolean;
  readonly RelationCondition: string;
  readonly DisplayFieldNames: string[];
  readonly PreloadedArticles: EntityObject[];
}

export function isRelationField(field: any): field is RelationFieldSchema {
  return isField(field) && field.ClassNames.includes("RelationFieldSchema");
}

export enum CloningMode {
  Ignore = "Ignore",
  UseExisting = "UseExisting",
  Copy = "Copy"
}

export enum UpdatingMode {
  Ignore = "Ignore",
  Update = "Update"
}

export interface SingleRelationFieldSchema extends RelationFieldSchema {}

export function isSingleRelationField(field: any): field is SingleRelationFieldSchema {
  return isRelationField(field) && field.ClassNames.includes("SingleRelationFieldSchema");
}

export interface MultiRelationFieldSchema extends RelationFieldSchema {
  readonly OrderByFieldName?: string;
  readonly MaxDataListItemCount?: number;
}

export function isMultiRelationField(field: any): field is MultiRelationFieldSchema {
  return isRelationField(field) && field.ClassNames.includes("MultiRelationFieldSchema");
}

export interface ExtensionFieldSchema extends FieldSchema {
  readonly Changeable: boolean;
  readonly ExtensionContents: {
    readonly [contentName: string]: ContentSchema;
  };
}

export function isExtensionField(field: any): field is ExtensionFieldSchema {
  return isField(field) && field.ClassNames.includes("ExtensionFieldSchema");
}

export interface EditorSchema {
  readonly Content: ContentSchema;
  Definitions: {
    readonly [partialContentName: string]: ContentSchema;
  };
}

export interface ContentSchemasById {
  readonly [contentName: string]: ContentSchema;
}
