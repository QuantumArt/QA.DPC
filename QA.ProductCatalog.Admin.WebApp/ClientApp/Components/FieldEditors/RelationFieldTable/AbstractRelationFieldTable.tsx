import React, { ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { RelationFieldSchema } from "Models/EditorSchemaModels";
import {
  AbstractRelationFieldEditor,
  FieldEditorProps,
  FieldSelector,
  EntityComparer
} from "../AbstractFieldEditor";
import "./RelationFieldTable.scss";

export interface RelationFieldTableProps extends FieldEditorProps {
  sortItems?: EntityComparer;
  sortItemsBy?: string | FieldSelector;
  displayFields?: (string | FieldSelector)[];
  relationActions?: ReactNode;
}

export abstract class AbstractRelationFieldTable extends AbstractRelationFieldEditor<
  RelationFieldTableProps
> {
  protected _displayFields: FieldSelector[];

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
