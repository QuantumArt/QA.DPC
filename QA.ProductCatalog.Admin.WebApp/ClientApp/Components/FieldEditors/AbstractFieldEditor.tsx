import React, { Component, ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
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
        className="pt-form-helper-text"
        rules={rules}
      />
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
            <label htmlFor={this.id} title={fieldSchema.FieldDescription || fieldSchema.FieldName}>
              {fieldSchema.FieldTitle || fieldSchema.FieldName}:
              {fieldSchema.IsRequired && (
                <span className="field-editor__label-required">&nbsp;*</span>
              )}
            </label>
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
        <Row>
          <Col md xlOffset={4} mdOffset={3}>
            {this.renderValidation(model, fieldSchema)}
          </Col>
        </Row>
      </Col>
    );
  }
}
