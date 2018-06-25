import React from "react";
import { Col, Row } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject, ExtensionObject, isArticleObject } from "Models/EditorDataModels";
import {
  EnumFieldSchema,
  PlainFieldSchema,
  ClassifierFieldSchema,
  NumericFieldSchema,
  StringFieldSchema,
  ExtensionFieldSchema
} from "Models/EditorSchemaModels";
import { Validate } from "Models/ValidatableMixin";
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
import { AbstractFieldEditor } from "./AbstractFieldEditor";
export { FileFieldEditor } from "./FileFieldEditor";
export { RelationFieldList } from "./RelationFieldList";
import "./FieldEditors.scss";

@observer
export class StringFieldEditor extends AbstractFieldEditor {
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

@observer
export class NumericFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: NumericFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <InputNumber
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          isInteger={fieldSchema.IsInteger}
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
          intent={model.hasVisibleErrors(fieldSchema.FieldName) ? Intent.DANGER : Intent.NONE}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}

@observer
export class BooleanFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <CheckBox
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
export class DateFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="date"
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
export class TimeFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="time"
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
export class DateTimeFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }

  render() {
    const { model, fieldSchema, validate } = this.props;
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
          {validate && <Validate model={model} name={fieldSchema.FieldName} rules={validate} />}
        </Row>
      </Col>
    );
  }
}

@observer
export class ClassifierFieldEditor extends AbstractFieldEditor {
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
          validate={fieldSchema.IsRequired && required}
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
export class EnumFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: EnumFieldSchema) {
    const options = fieldSchema.Items.map(item => ({ value: item.Value, label: item.Alias }));
    return fieldSchema.ShowAsRadioButtons ? (
      <Col xl={8} md={9}>
        <RadioGroup
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          disabled={fieldSchema.IsReadOnly}
          validate={fieldSchema.IsRequired && required}
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
          validate={fieldSchema.IsRequired && required}
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
export class ExtensionFieldEditor extends AbstractFieldEditor {
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
          validate={fieldSchema.IsRequired && required}
          className={cn({
            "pt-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
        {this.renderErrors(model, fieldSchema)}
      </Col>
    );
  }
}
