import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { RelationSelection, validateRelationSelection } from "Models/RelationSelection";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { DataSerializer } from "Services/DataSerializer";
import { DataContext } from "Services/DataContext";
import { isString } from "Utils/TypeChecks";
import { RenderArticle, FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldAccordionProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
  saveRelations?: RelationSelection;
  fieldEditors?: FieldsConfig;
  children?: RenderArticle | ReactNode;
}

export abstract class AbstractRelationFieldAccordion extends AbstractFieldEditor<
  RelationFieldAccordionProps
> {
  @inject protected _dataSerializer: DataSerializer;
  @inject protected _dataContext: DataContext;
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      saveRelations,
      displayFields = (fieldSchema as RelationFieldSchema).DisplayFieldNames || []
    } = this.props;
    if (DEBUG && saveRelations) {
      const contentSchema = (fieldSchema as RelationFieldSchema).Content;
      validateRelationSelection(contentSchema, saveRelations);
    }
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
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
          <Col md>{this.renderControls(model, fieldSchema)}</Col>
        </Row>
        <Row>
          <Col md xlOffset={2} mdOffset={3}>
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
