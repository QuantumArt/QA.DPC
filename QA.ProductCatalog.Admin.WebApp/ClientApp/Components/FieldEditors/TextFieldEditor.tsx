import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { PlainFieldSchema } from "Models/EditorSchemaModels";
import { TextArea } from "Components/FormControls/FormControls";
import { required } from "Utils/Validators";
import { AbstractFieldEditor } from "./AbstractFieldEditor";

@observer
export class TextFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={10} md={9}>
        <TextArea
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row>
          <Col xl={2} md={3} className="field-editor__label">
            <label htmlFor={this.id} title={fieldSchema.FieldDescription || fieldSchema.FieldName}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && <span className="field-editor__label-required"> *</span>}
            </label>
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col md xlOffset={2} mdOffset={3}>
            {this.renderValidation()}
          </Col>
        </Row>
      </Col>
    );
  }
}
