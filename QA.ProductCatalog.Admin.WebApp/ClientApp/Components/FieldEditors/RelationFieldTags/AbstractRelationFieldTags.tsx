import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { Validator } from "mst-validation-mixin";
import { EntityObject } from "Models/EditorDataModels";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { isNullOrWhiteSpace } from "Utils/TypeChecks";
import { ComputedCache } from "Utils/WeakCache";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector,
  EntityComparer
} from "../AbstractFieldEditor";
import "./RelationFieldTags.scss";

export interface RelationFieldTagsProps extends FieldEditorProps {
  filterItems?: (item: EntityObject) => boolean;
  validateItems?: Validator;
  sortItems?: EntityComparer;
  sortItemsBy?: string | FieldSelector;
  displayField?: string | FieldSelector;
  // custom actions
  relationActions?: () => ReactNode;
}

export abstract class AbstractRelationFieldTags extends AbstractRelationFieldEditor<
  RelationFieldTagsProps
> {
  protected _displayField: FieldSelector;
  protected _isHalfSize = false;
  protected _validationCache = new ComputedCache<EntityObject, string>();

  constructor(props: RelationFieldTagsProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    this._displayField = this.makeDisplayFieldSelector(props.displayField, fieldSchema);
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
