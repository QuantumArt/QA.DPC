import React, { Component, ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { ArticleObject, ExtensionObject } from "Models/EditorDataModels";
import { FieldSchema } from "Models/EditorSchemaModels";
import { Validator, Validate } from "mst-validation-mixin";
import { isArray } from "Utils/TypeChecks";
import { required } from "Utils/Validators";
import "./FieldEditors.scss";

export type FieldSelector = (model: ArticleObject | ExtensionObject) => any;

export interface FieldEditorProps {
  model: ArticleObject | ExtensionObject;
  fieldSchema: FieldSchema;
  validate?: Validator | Validator[];
}

export abstract class AbstractFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends Component<P> {
  protected id = `_${Math.random()
    .toString(36)
    .slice(2)}`;

  protected abstract renderField(
    model: ArticleObject | ExtensionObject,
    fieldSchema: FieldSchema
  ): ReactNode;

  protected renderValidation(
    model: ArticleObject | ExtensionObject,
    fieldSchema: FieldSchema
  ): ReactNode {
    const { validate } = this.props;
    const rules = [];
    if (validate) {
      if (isArray(validate)) {
        rules.push(...validate);
      } else {
        rules.push(validate);
      }
    }
    if (fieldSchema.IsRequired) {
      rules.push(required);
    }
    return (
      <Validate
        model={model}
        name={fieldSchema.FieldName}
        errorClassName="pt-form-helper-text"
        rules={rules}
      />
    );
  }

  protected renderLabel(model: ArticleObject | ExtensionObject, fieldSchema: FieldSchema) {
    return (
      <label
        htmlFor={this.id}
        title={fieldSchema.FieldName}
        className={cn("field-editor__label-text", {
          "field-editor__label-text--edited": model.isEdited(fieldSchema.FieldName)
        })}
      >
        {fieldSchema.IsRequired && <span className="field-editor__label-required">*&nbsp;</span>}
        {fieldSchema.FieldTitle || fieldSchema.FieldName}:
        {fieldSchema.FieldDescription && (
          <>
            &nbsp;<Icon icon="help" title={fieldSchema.FieldDescription} />
          </>
        )}
      </label>
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
        <Row>
          <Col xl={4} md={3} className="field-editor__label">
            {this.renderLabel(model, fieldSchema)}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col xl={4} md={3} className="field-editor__label" />
          <Col md>{this.renderValidation(model, fieldSchema)}</Col>
        </Row>
      </Col>
    );
  }
}
