import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject } from "ProductEditor/Models/EditorDataModels";
import { PlainFieldSchema } from "ProductEditor/Models/EditorSchemaModels";
import { TextArea } from "ProductEditor/Components/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";

@observer
export class TextFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <TextArea
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
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
          <Col xl={2} md={3} className="field-editor__label">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={2} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
