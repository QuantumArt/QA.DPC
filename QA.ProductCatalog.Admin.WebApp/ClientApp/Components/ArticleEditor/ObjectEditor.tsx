import React, { Component, StatelessComponent } from "react";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject, isArticleObject } from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  ExtensionFieldSchema,
  FieldExactTypes,
  isExtensionField,
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
  RelationFieldAccordion
} from "Components/FieldEditors/FieldEditors";
import { asc } from "Utils/Array/Sort";
import { isFunction, isObject } from "Utils/TypeChecks";
import "./ArticleEditor.scss";

export interface ObjectEditorProps {
  model: ArticleObject | ExtensionObject;
  contentSchema: ContentSchema;
  fields?: FieldsConfig;
}

export interface FieldsConfig {
  [field: string]: typeof IGNORE | FieldValue | FieldEditor | ContentsConfig;
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

export interface ContentsConfig {
  [content: string]: FieldsConfig;
}

function isContentsConfig(field: any): field is ContentsConfig {
  return isObject(field) && !isArticleObject(field) && Object.values(field).every(isObject);
}

interface ObjectEditorBlock {
  fieldSchema: FieldSchema;
  FieldEditor?: FieldEditor;
  contentsConfig?: ContentsConfig;
}

export abstract class ObjectEditor<P = {}> extends Component<ObjectEditorProps & P> {
  _editorBlocks: ObjectEditorBlock[] = [];

  constructor(props: ObjectEditorProps & P, context?: any) {
    super(props, context);
    const { contentSchema } = this.props;
    Object.values(contentSchema.Fields)
      .sort(asc(f => f.FieldOrder))
      .forEach(fieldSchema => {
        this.prepareFieldBlock(fieldSchema);

        if (isExtensionField(fieldSchema)) {
          this.prepareContentsBlock(fieldSchema);
        }
      });
  }

  private prepareFieldBlock(fieldSchema: FieldSchema) {
    const { model, fields } = this.props;
    const fieldName = fieldSchema.FieldName;

    if (fields && fieldName in fields) {
      const field = fields[fieldName];

      if (isFunction(field)) {
        this._editorBlocks.push({
          fieldSchema,
          FieldEditor: field
        });
      } else if (field !== IGNORE) {
        model[fieldName] = field;
      }
    } else {
      this._editorBlocks.push({
        fieldSchema,
        FieldEditor: this.getDefaultFieldEditor(fieldSchema)
      });
    }
  }

  private prepareContentsBlock(fieldSchema: ExtensionFieldSchema) {
    const { fields } = this.props;
    const contentsConfig = fields && fields[`${fieldSchema.FieldName}_Contents`];

    if (isContentsConfig(contentsConfig)) {
      this._editorBlocks.push({ fieldSchema, contentsConfig });
    } else if (contentsConfig !== IGNORE) {
      this._editorBlocks.push({ fieldSchema });
    }
  }

  private getDefaultFieldEditor(fieldSchema: FieldSchema): FieldEditor {
    if (isRelationField(fieldSchema)) {
      return RelationFieldAccordion;
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

  render(): JSX.Element | JSX.Element[] {
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
              fields={extensionFields}
            />
          );
        }

        return null;
      })
      .filter(Boolean);
  }
}

@observer
class ExtensionEditor extends ObjectEditor<{ model: ExtensionObject }> {}
