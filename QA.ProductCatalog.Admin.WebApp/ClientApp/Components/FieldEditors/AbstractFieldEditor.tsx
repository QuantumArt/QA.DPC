import React, { Component, ReactNode } from "react";
import { Col, Row } from "react-flexbox-grid";
import { inject } from "react-ioc";
import cn from "classnames";
import { Icon } from "@blueprintjs/core";
import { ArticleObject } from "Models/EditorDataModels";
import {
  FieldSchema,
  RelationFieldSchema,
  UpdatingMode,
  FieldExactTypes
} from "Models/EditorSchemaModels";
import { RelationController } from "Services/RelationController";
import { Validator, Validate } from "mst-validation-mixin";
import { isArray } from "Utils/TypeChecks";
import { required } from "Utils/Validators";
import { newUid } from "Utils/Common";
import "./FieldEditors.scss";

export type FieldSelector<T = ReactNode> = (model: ArticleObject) => T;

export interface FieldEditorProps {
  model: ArticleObject;
  fieldSchema: FieldSchema;
  validate?: Validator | Validator[];
  readonly?: boolean;
}

export abstract class AbstractFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends Component<P> {
  protected _id = `_${newUid()}`;
  protected _readonly = this.props.readonly || this.props.fieldSchema.IsReadOnly;

  protected abstract renderField(model: ArticleObject, fieldSchema: FieldSchema): ReactNode;

  protected renderValidation(model: ArticleObject, fieldSchema: FieldSchema): ReactNode {
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
        errorClassName="bp3-form-helper-text"
        rules={rules}
      />
    );
  }

  protected renderLabel(model: ArticleObject, fieldSchema: FieldSchema) {
    return (
      <label
        htmlFor={this._id}
        title={fieldSchema.FieldName}
        className={cn("field-editor__label-text", {
          "field-editor__label-text--edited": model.isEdited(fieldSchema.FieldName),
          "field-editor__label-text--invalid": model.hasVisibleErrors(fieldSchema.FieldName)
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
        className={cn("field-editor__block bp3-form-group", {
          "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
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

export abstract class AbstractRelationFieldEditor<
  P extends FieldEditorProps = FieldEditorProps
> extends AbstractFieldEditor<P> {
  @inject protected _relationController: RelationController;

  constructor(props: P, context?: any) {
    super(props, context);
    const fieldSchema = this.props.fieldSchema as RelationFieldSchema;
    this._readonly =
      this._readonly ||
      (fieldSchema.FieldType === FieldExactTypes.M2ORelation &&
        fieldSchema.UpdatingMode === UpdatingMode.Ignore);
  }
}
