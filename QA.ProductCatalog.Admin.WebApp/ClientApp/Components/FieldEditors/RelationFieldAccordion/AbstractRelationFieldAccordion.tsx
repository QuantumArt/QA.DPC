import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { SchemaContext } from "Services/SchemaContext";
import { RelationController } from "Services/RelationController";
import { isString } from "Utils/TypeChecks";
import { RenderArticle, FieldsConfig } from "Components/ArticleEditor/ArticleEditor";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

export interface RelationFieldAccordionProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
  fieldEditors?: FieldsConfig;
  filterItems?: (item: EntityObject) => boolean;
  children?: RenderArticle | ReactNode;
}

export abstract class AbstractRelationFieldAccordion extends AbstractFieldEditor<
  RelationFieldAccordionProps
> {
  @inject protected _dataContext: DataContext;
  @inject private _schemaContext: SchemaContext;
  @inject protected _relationController: RelationController;
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayFields = (fieldSchema as RelationFieldSchema).DisplayFieldNames || []
    } = this.props;
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
  }

  protected showSaveButton(article: EntityObject) {
    const { fieldSchema } = this.props;
    const contentSchema = (fieldSchema as RelationFieldSchema).Content;
    return article._ServerId > 0 || this._schemaContext.rootSchema === contentSchema;
  }

  protected abstract renderControls(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

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
            {this.renderLabel(model, fieldSchema)}
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
