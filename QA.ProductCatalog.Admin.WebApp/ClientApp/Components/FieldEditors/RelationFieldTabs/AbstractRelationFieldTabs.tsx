import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { RelationSelection, validateRelationSelection } from "Models/RelationSelection";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { isString } from "Utils/TypeChecks";
import { RenderArticle, FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldTabs.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldTabsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  saveRelations?: RelationSelection;
  fieldEditors?: FieldsConfig;
  collapsed?: boolean;
  vertical?: boolean;
  children?: RenderArticle | ReactNode;
}

export abstract class AbstractRelationFieldTabs extends AbstractFieldEditor<
  RelationFieldTabsProps
> {
  @inject protected _dataContext: DataContext;
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldTabsProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      saveRelations,
      displayField = (fieldSchema as RelationFieldSchema).Content.DisplayFieldName || "Id"
    } = this.props;
    if (DEBUG && saveRelations) {
      const contentSchema = (fieldSchema as RelationFieldSchema).Content;
      validateRelationSelection(contentSchema, saveRelations);
    }
    this._displayField = isString(displayField)
      ? displayField === "Id"
        ? () => ""
        : article => article[displayField]
      : displayField;
  }

  protected abstract renderControls(
    model: ArticleObject | ExtensionObject,
    fieldSchema: FieldSchema
  ): ReactNode;

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        md={12}
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label field-editor__label--small">
            <label htmlFor={this.id} title={fieldSchema.FieldDescription || fieldSchema.FieldName}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && (
                <span className="field-editor__label-required">&nbsp;*</span>
              )}
            </label>
          </Col>
          <Col md>
            {this.renderControls(model, fieldSchema)}
            {this.renderValidation(model, fieldSchema)}
          </Col>
        </Row>
        <Row>
          <Col md>{this.renderField(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
