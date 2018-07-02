import React, { Component, StatelessComponent } from "react";
import { consumer, inject } from "react-ioc";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject, isArticleObject } from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  ExtensionFieldSchema,
  FieldExactTypes,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  isRelationField
} from "Models/EditorSchemaModels";
import {
  ExtensionFieldEditor,
  StringFieldEditor,
  BooleanFieldEditor,
  NumericFieldEditor,
  DateFieldEditor,
  TimeFieldEditor,
  DateTimeFieldEditor,
  FileFieldEditor,
  TextFieldEditor,
  ClassifierFieldEditor,
  EnumFieldEditor,
  SingleRelationFieldAccordion,
  MultiRelationFieldAccordion
} from "Components/FieldEditors/FieldEditors";
import { asc } from "Utils/Array/Sort";
import { isFunction, isObject } from "Utils/TypeChecks";
import "./ArticleEditor.scss";

export class RelationsConfig {
  [contentName: string]: typeof IGNORE | FieldEditor;
}

export interface FieldsConfig {
  [fieldName: string]: typeof IGNORE | FieldValue | FieldEditor | ContentsConfig;
}

export interface ContentsConfig {
  [contentName: string]: FieldsConfig;
}

function isContentsConfig(field: any): field is ContentsConfig {
  return isObject(field) && !isArticleObject(field) && Object.values(field).every(isObject);
}

export const IGNORE = Symbol("IGNORE");

type FieldValue =
  | null
  | string
  | string[]
  | number
  | number[]
  | boolean
  | Date
  | ArticleObject
  | ArticleObject[];

interface FieldEditorProps {
  model: ArticleObject | ExtensionObject;
  fieldSchema: FieldSchema;
}

declare class FieldEditorComponent extends Component<FieldEditorProps> {}
type FieldEditor = StatelessComponent<FieldEditorProps> | typeof FieldEditorComponent;

export interface ObjectEditorProps {
  model: ArticleObject | ExtensionObject;
  contentSchema: ContentSchema;
  fieldEdiors?: FieldsConfig;
}

interface ObjectEditorBlock {
  fieldSchema: FieldSchema;
  FieldEditor?: FieldEditor;
  contentsConfig?: ContentsConfig;
}

export abstract class ObjectEditor<P = {}> extends Component<ObjectEditorProps & P> {
  @inject private _relationsConfig: RelationsConfig;
  private _editorBlocks: ObjectEditorBlock[] = [];

  constructor(props: ObjectEditorProps & P, context?: any) {
    super(props, context);
    const { contentSchema, children } = this.props;
    console.time("ObjectEditor constructor");
    // TODO: cache by contentSchema and memoize by props.fieldEdiors
    if (!isFunction(children) || children.length > 0) {
      Object.values(contentSchema.Fields)
        .sort(asc(f => f.FieldOrder))
        .forEach(fieldSchema => {
          this.prepareFieldBlock(fieldSchema);

          if (isExtensionField(fieldSchema)) {
            this.prepareContentsBlock(fieldSchema);
          }
        });
    }
    console.timeEnd("ObjectEditor constructor");
  }

  private prepareFieldBlock(fieldSchema: FieldSchema) {
    const { model, fieldEdiors } = this.props;
    const fieldName = fieldSchema.FieldName;

    if (fieldEdiors && fieldEdiors.hasOwnProperty(fieldName)) {
      const field = fieldEdiors[fieldName];

      if (isFunction(field)) {
        this._editorBlocks.push({
          fieldSchema,
          FieldEditor: field
        });
      } else if (field !== IGNORE) {
        model[fieldName] = field;
      }
      return;
    }
    if (isRelationField(fieldSchema)) {
      const contentName = fieldSchema.Content.ContentName;
      if (this._relationsConfig.hasOwnProperty(contentName)) {
        const field = this._relationsConfig[contentName];

        if (isFunction(field)) {
          this._editorBlocks.push({
            fieldSchema,
            FieldEditor: field
          });
        } else if (field !== IGNORE) {
          model[fieldName] = field;
        }
        return;
      }
    }
    this._editorBlocks.push({
      fieldSchema,
      FieldEditor: this.getDefaultFieldEditor(fieldSchema)
    });
  }

  private prepareContentsBlock(fieldSchema: ExtensionFieldSchema) {
    const { fieldEdiors } = this.props;
    const contentsConfig = fieldEdiors && fieldEdiors[`${fieldSchema.FieldName}_Contents`];

    if (isContentsConfig(contentsConfig)) {
      this._editorBlocks.push({ fieldSchema, contentsConfig });
    } else if (contentsConfig !== IGNORE) {
      this._editorBlocks.push({ fieldSchema });
    }
  }

  private getDefaultFieldEditor(fieldSchema: FieldSchema): FieldEditor {
    if (isSingleRelationField(fieldSchema)) {
      return SingleRelationFieldAccordion;
    }
    if (isMultiRelationField(fieldSchema)) {
      return MultiRelationFieldAccordion;
    }
    if (isExtensionField(fieldSchema)) {
      return ExtensionFieldEditor;
    }
    switch (fieldSchema.FieldType) {
      case FieldExactTypes.String:
        return StringFieldEditor;
      case FieldExactTypes.Numeric:
        return NumericFieldEditor;
      case FieldExactTypes.Boolean:
        return BooleanFieldEditor;
      case FieldExactTypes.Date:
        return DateFieldEditor;
      case FieldExactTypes.Time:
        return TimeFieldEditor;
      case FieldExactTypes.DateTime:
        return DateTimeFieldEditor;
      case FieldExactTypes.File:
      case FieldExactTypes.Image:
        return FileFieldEditor;
      case FieldExactTypes.Textbox:
      case FieldExactTypes.VisualEdit:
        return TextFieldEditor;
      case FieldExactTypes.Classifier:
        return ClassifierFieldEditor;
      case FieldExactTypes.StringEnum:
        return EnumFieldEditor;
    }
    throw new Error(`Unsupported field type FieldExactTypes.${fieldSchema.FieldType}`);
  }

  render() {
    const { model } = this.props;

    return this._editorBlocks
      .map(({ fieldSchema, FieldEditor, contentsConfig }) => {
        const fieldName = fieldSchema.FieldName;
        if (FieldEditor) {
          return <FieldEditor key={fieldName} model={model} fieldSchema={fieldSchema} />;
        }

        const contentName: string = model[fieldName];
        if (contentName) {
          const extensionModel = model[`${fieldName}_Contents`][contentName];
          const extensionSchema = (fieldSchema as ExtensionFieldSchema).Contents[contentName];
          const extensionFields = contentsConfig && contentsConfig[contentName];
          return (
            <ExtensionEditor
              key={fieldName + "_" + contentName}
              model={extensionModel}
              contentSchema={extensionSchema}
              fieldEdiors={extensionFields}
            />
          );
        }

        return null;
      })
      .filter(Boolean);
  }
}

@consumer
@observer
class ExtensionEditor extends ObjectEditor<{ model: ExtensionObject }> {}
