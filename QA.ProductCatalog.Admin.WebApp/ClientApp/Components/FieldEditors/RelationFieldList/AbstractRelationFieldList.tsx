import React, { MouseEvent } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { ArticleObject } from "Models/EditorDataModels";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import { AbstractFieldEditor, FieldEditorProps, FieldSelector } from "../AbstractFieldEditor";
import "./RelationFieldList.scss";

// TODO: Интеграция с окном выбора статей QP
// TODO: Загрузка части продукта, которая начинается с новой выбранной статьи

export interface RelationFieldListProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  selectMultiple?: boolean;
  onClick?: (e: MouseEvent<HTMLElement>, article: ArticleObject) => void;
}

export abstract class AbstractRelationFieldList extends AbstractFieldEditor<
  RelationFieldListProps
> {
  protected _displayField: FieldSelector;

  constructor(props: RelationFieldListProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayField = (fieldSchema as RelationFieldSchema).Content.DisplayFieldName || "Id"
    } = this.props;
    this._displayField = isString(displayField)
      ? displayField === "Id"
        ? () => ""
        : article => article[displayField]
      : displayField;
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
            <label htmlFor={this.id} title={fieldSchema.FieldDescription || fieldSchema.FieldName}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && (
                <span className="field-editor__label-required">&nbsp;*</span>
              )}
            </label>
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col md xlOffset={2} mdOffset={3}>
            {this.renderValidation(model, fieldSchema)}
          </Col>
        </Row>
      </Col>
    );
  }
}
