import React, { ReactNode } from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema, FieldSchema } from "Models/EditorSchemaModels";
import { ArticleObject, EntityObject } from "Models/EditorDataModels";
import { DataContext } from "Services/DataContext";
import { EditorController } from "Services/EditorController";
import { ArticleController } from "Services/ArticleController";
import { isString } from "Utils/TypeChecks";
import { RenderEntity, FieldsConfig } from "Components/ArticleEditor/EntityEditor";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldAccordion.scss";

export interface RelationFieldAccordionProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
  fieldOrders?: string[];
  fieldEditors?: FieldsConfig;
  filterItems?: (item: EntityObject) => boolean;
  collapsed?: boolean;
  children?: RenderEntity | ReactNode;
}

export abstract class AbstractRelationFieldAccordion extends AbstractRelationFieldEditor<
  RelationFieldAccordionProps
> {
  @inject protected _dataContext: DataContext;
  @inject protected _articleController: ArticleController;
  @inject protected _editorController: EditorController;
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldAccordionProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayFields = props.displayFields || fieldSchema.DisplayFieldNames || [];
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
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
