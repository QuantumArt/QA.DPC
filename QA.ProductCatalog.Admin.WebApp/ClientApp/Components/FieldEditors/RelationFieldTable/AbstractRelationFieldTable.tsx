import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { Validator } from "mst-validation-mixin";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import { EntityObject } from "Models/EditorDataModels";
import { ComputedCache } from "Utils/WeakCache";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector,
  EntityComparer,
  HighlightMode
} from "../AbstractFieldEditor";
import "./RelationFieldTable.scss";

export interface RelationFieldTableProps extends FieldEditorProps {
  filterItems?: (item: EntityObject) => boolean;
  highlightItems?: (item: EntityObject) => HighlightMode;
  validateItem?: Validator;
  sortItems?: EntityComparer;
  sortItemsBy?: string | FieldSelector;
  displayFields?: (string | FieldSelector)[];
  // custom actions
  relationActions?: () => ReactNode;
}

export abstract class AbstractRelationFieldTable extends AbstractRelationFieldEditor<
  RelationFieldTableProps
> {
  protected _displayFields: FieldSelector[];
  protected _validationCache = new ComputedCache<EntityObject, string>();

  constructor(props: RelationFieldTableProps, context?: any) {
    super(props, context);
    const fieldSchema = props.fieldSchema as RelationFieldSchema;
    this._displayFields = this.makeDisplayFieldsSelectors(props.displayFields, fieldSchema);
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        md={12}
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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
