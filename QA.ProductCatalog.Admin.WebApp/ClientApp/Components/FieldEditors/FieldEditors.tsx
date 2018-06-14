import "./FieldEditors.scss";
import React, { Component } from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject, isArticleObject } from "Models/EditorDataModels";
import {
  FieldSchema,
  EnumFieldSchema,
  PlainFieldSchema,
  ClassifierFieldSchema,
  NumericFieldSchema,
  StringFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import {
  InputText,
  RadioGroup,
  Select,
  InputNumber,
  TextArea,
  CheckBox,
  DatePicker
} from "Components/FormControls/FormControls";
import { required, pattern } from "Utils/Validators";
import { Intent } from "@blueprintjs/core";

interface FieldEditorProps<TSchema extends FieldSchema> {
  model: ArticleObject | ExtensionObject;
  fieldSchema: TSchema;
}

export abstract class AbstractFieldEditor<TSchema extends FieldSchema> extends Component<
  FieldEditorProps<TSchema>
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

@observer
export class StringFieldEditor extends AbstractFieldEditor<StringFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: StringFieldSchema) {
    return (
      <Col xl={8} md={6}>
        <InputText
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={[
            fieldSchema.IsRequired && required,
            fieldSchema.RegexPattern && pattern(fieldSchema.RegexPattern)
          ]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

// TODO: remove fake validation from other FieldEditors

@observer
export class NumericFieldEditor extends AbstractFieldEditor<NumericFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: NumericFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <InputNumber
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          isInteger={fieldSchema.IsInteger}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          intent={model.hasVisibleErrors(fieldSchema.FieldName) ? Intent.DANGER : Intent.NONE}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class BooleanFieldEditor extends AbstractFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <CheckBox
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class DateFieldEditor extends AbstractFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="date"
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class TimeFieldEditor extends AbstractFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="time"
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class DateTimeFieldEditor extends AbstractFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

// TODO: buttons & markup
@observer
export class FileFieldEditor extends AbstractFieldEditor<StringFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: StringFieldSchema) {
    return (
      <Col xl={8} md={6}>
        <div
          className={cn("pt-input-group pt-fill", {
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        >
          <InputText
            id={this.id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
            readOnly
            validate={[required, pattern(/^[A-Za-z]+$/)]}
          />
          <button className="pt-button pt-minimal pt-intent-warning pt-icon-cloud-download" />
        </div>
        {model.hasVisibleErrors(fieldSchema.FieldName) && (
          <div className="pt-form-helper-text">
            {model
              .getVisibleErrors(fieldSchema.FieldName)
              .map((error, i) => <div key={i}>{error}</div>)}
          </div>
        )}
      </Col>
    );
  }
}

@observer
export class TextFieldEditor extends AbstractFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={10} md={9}>
        <TextArea
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[A-Za-z]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
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
        <Row middle="xs">
          <Col xl={2} md={3}>
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

@observer
export class ClassifierFieldEditor extends AbstractFieldEditor<ClassifierFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: ClassifierFieldSchema) {
    const value = model[fieldSchema.FieldName];
    const options = value ? [{ value, label: value }] : [];
    return (
      <Col xl={8} md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required
          disabled
          validate={[required, pattern(/^[0-9]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class EnumFieldEditor extends AbstractFieldEditor<EnumFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: EnumFieldSchema) {
    const options = fieldSchema.Items.map(item => ({ value: item.Value, label: item.Alias }));
    return fieldSchema.ShowAsRadioButtons ? (
      <Col xl={8} md={9}>
        <RadioGroup
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[0-9]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    ) : (
      <Col xl={8} md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required={fieldSchema.IsRequired}
          disabled={fieldSchema.IsReadOnly}
          validate={[required, pattern(/^[0-9]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class ExtensionFieldEditor extends AbstractFieldEditor<ExtensionFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: ExtensionFieldSchema) {
    const options = Object.values(fieldSchema.Contents).map(contentSchema => ({
      value: contentSchema.ContentName,
      label: contentSchema.ContentTitle || contentSchema.ContentName
    }));
    const disabled =
      fieldSchema.IsReadOnly ||
      (!fieldSchema.Changeable && isArticleObject(model) && model.Modified instanceof Date);
    return (
      <Col xl={8} md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required={fieldSchema.IsRequired}
          disabled={disabled}
          validate={[required, pattern(/^[0-9]+$/)]}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}
