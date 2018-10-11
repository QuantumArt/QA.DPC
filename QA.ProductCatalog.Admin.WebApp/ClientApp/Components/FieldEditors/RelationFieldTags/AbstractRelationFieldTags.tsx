import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { EntityObject } from "Models/EditorDataModels";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { isString, isNullOrWhiteSpace } from "Utils/TypeChecks";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldTags.scss";

export interface RelationFieldTagsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
}

export abstract class AbstractRelationFieldTags extends AbstractRelationFieldEditor<
  RelationFieldTagsProps
> {
  protected _displayField: FieldSelector;
  protected _isHalfSize = false;

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    const displayField =
      props.displayField || fieldSchema.RelatedContent.DisplayFieldName || (() => "");
    this._displayField = isString(displayField) ? entity => entity[displayField] : displayField;
  }

  protected getTitle(entity: EntityObject) {
    const title = this._displayField(entity);
    return isNullOrWhiteSpace(title) ? "..." : title;
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={this._isHalfSize ? 6 : 12}
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col
            xl={this._isHalfSize ? 4 : 2}
            md={3}
            className="field-editor__label field-editor__label--small"
          >
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={this._isHalfSize ? 4 : 2} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
