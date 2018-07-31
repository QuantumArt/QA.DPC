import React from "react";
import { inject } from "react-ioc";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { RelationController } from "Services/RelationController";
import { isString } from "Utils/TypeChecks";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldTable.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldTableProps extends FieldEditorProps {
  displayFields?: (string | FieldSelector)[];
  orderByField?: string | FieldSelector;
}

export abstract class AbstractRelationFieldTable extends AbstractFieldEditor<
  RelationFieldTableProps
> {
  @inject protected _relationController: RelationController;
  protected _displayFields: FieldSelector[];

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayFields = (fieldSchema as RelationFieldSchema).DisplayFieldNames || []
    } = this.props;
    this._displayFields = displayFields.map(
      field => (isString(field) ? article => article[field] : field)
    );
  }

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
          {this.renderField(model, fieldSchema)}
        </Row>
      </Col>
    );
  }
}
