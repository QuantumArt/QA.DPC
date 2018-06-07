import React, { Component, StatelessComponent } from "react";
import { Row } from "react-flexbox-grid";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject, isArticleObject } from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  isExtensionField,
  ExtensionFieldSchema,
  FieldExactTypes
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
  EnumFieldEditor
} from "Components/FieldEditors/FieldEditors";
import { asc } from "Utils/Array/Sort";
import { isFunction, isObject } from "Utils/TypeChecks";

interface ObjectEditorProps {
  model: ArticleObject | ExtensionObject;
  contentSchema: ContentSchema;
  fields?: FieldsConfig;
}

interface FieldsConfig {
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

abstract class ObjectEditor<P> extends Component<ObjectEditorProps & P> {
  _fieldSchemas: FieldSchema[] = [];
  _fieldEditors: FieldEditor[] = [];
  _contentsConfigs: {
    [field: string]: ContentsConfig;
  } = Object.create(null);

  constructor(props: ObjectEditorProps & P, context?: any) {
    super(props, context);
    const { contentSchema } = this.props;
    Object.values(contentSchema.Fields)
      .sort(asc(f => f.FieldOrder))
      .forEach(fieldSchema => {
        if (isExtensionField(fieldSchema)) {
          this.prepareExtensionField(fieldSchema);
        } else {
          this.prepareRegularField(fieldSchema);
        }
      });
  }

  private prepareExtensionField(fieldSchema: ExtensionFieldSchema) {
    const { fields } = this.props;
    const fieldName = fieldSchema.FieldName;
    if (fields) {
      const field = fields[fieldName];
      if (isContentsConfig(field)) {
        this._contentsConfigs[fieldName] = field;
      } else {
        this.prepareRegularField(fieldSchema);
      }
      const contents = fields[`${fieldName}_Contents`];
      if (isContentsConfig(contents)) {
        this._contentsConfigs[fieldName] = contents;
      }
    }
  }

  private prepareRegularField(fieldSchema: FieldSchema) {
    const { model, fields } = this.props;
    const fieldName = fieldSchema.FieldName;
    if (fields && fieldName in fields) {
      const field = fields[fieldName];
      if (isFunction(field)) {
        this._fieldEditors.push(field);
        this._fieldSchemas.push(fieldSchema);
      } else if (field !== IGNORE) {
        model[fieldName] = field;
      }
    } else {
      // TODO: убрать, когда будут реализованы редакторы связей
      try {
        this._fieldEditors.push(this.getDefaultFieldEditor(fieldSchema));
        this._fieldSchemas.push(fieldSchema);
      } catch (e) {
        console.error(e.message);
      }
    }
  }

  private getDefaultFieldEditor(fieldSchema: FieldSchema): FieldEditor {
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
    const fragments: JSX.Element[] = [];
    this._fieldEditors.forEach((FieldEditor, i) => {
      const fieldSchema = this._fieldSchemas[i];
      const fieldName = fieldSchema.FieldName;

      fragments.push(<FieldEditor key={fieldName} model={model} fieldSchema={fieldSchema} />);

      if (isExtensionField(fieldSchema)) {
        const contentName: string = model[fieldName];
        if (contentName) {
          const extensionModel = model[`${fieldName}_Contents`][contentName];
          const extensionSchema = fieldSchema.Contents[contentName];

          let extensionFields: FieldsConfig;
          const contentsConfig = this._contentsConfigs[fieldName];
          if (contentsConfig) {
            extensionFields = contentsConfig[contentName];
          }

          fragments.push(
            <ExtensionEditor
              key={fieldName + "_" + contentName}
              model={extensionModel}
              contentSchema={extensionSchema}
              fields={extensionFields}
            />
          );
        }
      }
    });
    return fragments;
  }
}

@observer
export class ArticleEditor extends ObjectEditor<{ model: ArticleObject }> {
  render() {
    return <Row>{super.render()}</Row>;
  }
}

@observer
export class ExtensionEditor extends ObjectEditor<{ model: ExtensionObject }> {}
