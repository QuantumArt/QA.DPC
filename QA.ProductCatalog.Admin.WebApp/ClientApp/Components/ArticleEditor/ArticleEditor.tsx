import React, { Component, StatelessComponent, ReactNode } from "react";
import { consumer, inject } from "react-ioc";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import {
  ContentSchema,
  FieldSchema,
  ExtensionFieldSchema,
  RelationFieldSchema,
  FieldExactTypes,
  isExtensionField,
  isSingleRelationField,
  isMultiRelationField,
  isRelationField,
  PreloadingMode
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
  RelationFieldSelect,
  RelationFieldAccordion,
  RelationFieldCheckList,
  RelationFieldForm
} from "Components/FieldEditors/FieldEditors";
import { asc } from "Utils/Array/Sort";
import { isFunction, isObject } from "Utils/TypeChecks";
import "./ArticleEditor.scss";

export class RelationsConfig {
  [contentName: string]: typeof IGNORE | RelationFieldEditor;
}

export interface FieldsConfig {
  [fieldName: string]: typeof IGNORE | FieldValue | FieldEditor | ExtensionConfig;
}

interface ExtensionConfig {
  [contentName: string]: FieldsConfig;
}

function isContentsConfig(field: any): field is ExtensionConfig {
  return isObject(field) && Object.values(field).every(isObject);
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
  | EntityObject
  | EntityObject[];

export interface FieldEditorProps {
  model: ArticleObject;
  fieldSchema: FieldSchema;
}

declare class FieldEditorComponent extends Component<FieldEditorProps> {}
type FieldEditor = StatelessComponent<FieldEditorProps> | typeof FieldEditorComponent;

interface RelationFieldEditorProps extends FieldEditorProps {
  fieldSchema: RelationFieldSchema;
}

declare class RelationFieldEditorComponent extends Component<FieldEditorProps> {}
type RelationFieldEditor =
  | StatelessComponent<RelationFieldEditorProps>
  | typeof RelationFieldEditorComponent;

export interface ArticleEditorProps {
  model: ArticleObject;
  contentSchema: ContentSchema;
  fieldOrders?: string[];
  fieldEditors?: FieldsConfig;
  skipOtherFields?: boolean;
}

interface ObjectEditorBlock {
  fieldSchema: FieldSchema;
  FieldEditor?: FieldEditor;
  contentsConfig?: ExtensionConfig;
}

export abstract class AbstractEditor<P extends ArticleEditorProps> extends Component<P> {
  @inject private _relationsConfig: RelationsConfig;
  private _editorBlocks: ObjectEditorBlock[] = [];

  constructor(props: ArticleEditorProps & P, context?: any) {
    super(props, context);
    this.prepareFields();
  }

  @action
  private prepareFields() {
    const { contentSchema, fieldOrders, children } = this.props;
    if (isFunction(children) && children.length === 0) {
      return;
    }
    const fieldOrderByName = {};
    if (fieldOrders) {
      fieldOrders
        .slice()
        .reverse()
        .forEach((fieldName, i) => {
          fieldOrderByName[fieldName] = -(i + 1);
        });
    }
    // TODO: cache by contentSchema and memoize by props.fieldEditors
    Object.values(contentSchema.Fields)
      .sort(asc(field => fieldOrderByName[field.FieldName] || field.FieldOrder))
      .forEach(fieldSchema => {
        if (this.shouldIncludeField(fieldSchema)) {
          this.prepareFieldBlock(fieldSchema);

          if (isExtensionField(fieldSchema)) {
            this.prepareContentsBlock(fieldSchema);
          }
        }
      });
  }

  private shouldIncludeField(fieldSchema: FieldSchema) {
    const { skipOtherFields, fieldEditors } = this.props;
    return !skipOtherFields || (fieldEditors && fieldEditors.hasOwnProperty(fieldSchema.FieldName));
  }

  private prepareFieldBlock(fieldSchema: FieldSchema) {
    const { model, fieldEditors } = this.props;
    const fieldName = fieldSchema.FieldName;

    if (fieldEditors && fieldEditors.hasOwnProperty(fieldName)) {
      const field = fieldEditors[fieldName];

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
    if (fieldSchema.IsReadOnly) {
      return;
    }
    if (isRelationField(fieldSchema)) {
      const contentName = fieldSchema.RelatedContent.ContentName;
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
    const { fieldEditors } = this.props;
    const contentsConfig = fieldEditors && fieldEditors[`${fieldSchema.FieldName}_Extension`];

    if (isContentsConfig(contentsConfig)) {
      this._editorBlocks.push({ fieldSchema, contentsConfig });
    } else if (contentsConfig !== IGNORE) {
      this._editorBlocks.push({ fieldSchema });
    }
  }

  private getDefaultFieldEditor(fieldSchema: FieldSchema): FieldEditor {
    if (isSingleRelationField(fieldSchema)) {
      if (fieldSchema.PreloadingMode !== PreloadingMode.None) {
        return RelationFieldSelect;
      }
      return RelationFieldForm;
    }
    if (isMultiRelationField(fieldSchema)) {
      if (fieldSchema.PreloadingMode !== PreloadingMode.None) {
        return RelationFieldCheckList;
      }
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

  render(): ReactNode {
    const { model, fieldOrders } = this.props;

    return this._editorBlocks
      .map(({ fieldSchema, FieldEditor, contentsConfig }) => {
        const fieldName = fieldSchema.FieldName;
        if (FieldEditor) {
          return <FieldEditor key={fieldName} model={model} fieldSchema={fieldSchema} />;
        }

        const contentName: string = model[fieldName];
        if (contentName) {
          const extensionModel = model[`${fieldName}${ArticleObject._Extension}`][contentName];
          const extensionSchema = (fieldSchema as ExtensionFieldSchema).ExtensionContents[
            contentName
          ];
          const extensionFields = contentsConfig && contentsConfig[contentName];
          return (
            <ArticleEditor
              key={fieldName + "_" + contentName}
              model={extensionModel}
              fieldOrders={fieldOrders}
              contentSchema={extensionSchema}
              fieldEditors={extensionFields}
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
export class ArticleEditor extends AbstractEditor<ArticleEditorProps> {}
