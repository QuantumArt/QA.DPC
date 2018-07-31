import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { PlainFieldSchema } from "Models/EditorSchemaModels";
import { TextArea } from "Components/FormControls/FormControls";
import { AbstractFieldEditor } from "./AbstractFieldEditor";

@observer
export class TextFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <TextArea
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-primary": model.isEdited(fieldSchema.FieldName),
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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
