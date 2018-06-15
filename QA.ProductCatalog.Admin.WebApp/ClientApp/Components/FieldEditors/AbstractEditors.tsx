import React, { Component } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { FieldSchema } from "Models/EditorSchemaModels";

interface FieldEditorProps<TSchema extends FieldSchema> {
  model: ArticleObject | ExtensionObject;
  fieldSchema: TSchema;
}

export abstract class AbstractFieldEditor<TSchema extends FieldSchema, P = {}> extends Component<
  FieldEditorProps<TSchema> & P
> {
  protected id = `_${Math.random()
    .toString(36)
    .slice(2)}`;

  abstract renderField(model: ArticleObject | ExtensionObject, fieldSchema: TSchema);

  protected renderErrors(model: ArticleObject | ExtensionObject, fieldSchema: TSchema) {
    return (
      model.hasVisibleErrors(fieldSchema.FieldName) && (
        <div className="pt-form-helper-text">
          {model
            .getVisibleErrors(fieldSchema.FieldName)
            .map((error, i) => <div key={i}>{error}</div>)}
        </div>
      )
    );
  }

  protected renderErrorsStub(model: ArticleObject | ExtensionObject, fieldSchema: TSchema) {
    return (
      model.hasVisibleErrors(fieldSchema.FieldName) && (
        <div className="pt-form-helper-text" style={{ visibility: "hidden" }}>
          {model.getVisibleErrors(fieldSchema.FieldName).map((_, i) => <div key={i}>*</div>)}
        </div>
      )
    );
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col
        xl={6}
        md={12}
        className={cn("field-editor__block pt-form-group", {
          "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
        })}
      >
        <Row middle="xs">
          <Col xl={4} md={3}>
            <label htmlFor={this.id} title={fieldSchema.FieldDescription}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && <span className="field-editor__label--required"> *</span>}
            </label>
            {this.renderErrorsStub(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
      </Col>
    );
  }
}
