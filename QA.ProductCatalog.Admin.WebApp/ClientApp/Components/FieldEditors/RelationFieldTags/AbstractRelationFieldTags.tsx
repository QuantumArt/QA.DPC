import React, { MouseEvent } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { EntityObject } from "Models/EditorDataModels";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { isString } from "Utils/TypeChecks";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector
} from "../AbstractFieldEditor";
import "./RelationFieldTags.scss";

export interface RelationFieldTagsProps extends FieldEditorProps {
  displayField?: string | FieldSelector;
  orderByField?: string | FieldSelector;
  selectMultiple?: boolean;
  onClick?: (e: MouseEvent<HTMLElement>, article: EntityObject) => void;
}

export abstract class AbstractRelationFieldTags extends AbstractRelationFieldEditor<
  RelationFieldTagsProps
> {
  protected _displayField: FieldSelector;
  protected _canEditRelation: boolean;
  protected _isHalfSize = false;

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const {
      fieldSchema,
      displayField = (fieldSchema as RelationFieldSchema).RelatedContent.DisplayFieldName ||
        (() => "")
    } = this.props;
    this._displayField = isString(displayField) ? article => article[displayField] : displayField;
  }

  protected getTitle(article: EntityObject) {
    const title = this._displayField(article);
    return title != null && !/^\s*$/.test(title) ? title : "...";
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={this._isHalfSize ? 6 : 12}
        md={12}
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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
