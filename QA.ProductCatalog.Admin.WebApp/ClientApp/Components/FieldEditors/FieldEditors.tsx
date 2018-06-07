import "./FieldEditors.scss";
import React, { Component } from "react";
import { Col, Row } from "react-flexbox-grid";
import { Tooltip, Position } from "@blueprintjs/core";
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

interface FieldEditorProps<TSchema extends FieldSchema> {
  model: ArticleObject | ExtensionObject;
  fieldSchema: TSchema;
}

abstract class PlainFieldEditor<TSchema extends PlainFieldSchema> extends Component<
  FieldEditorProps<TSchema>
> {
  protected id = `_${Math.random()
    .toString(36)
    .slice(2)}`;

  abstract renderField(model: ArticleObject | ExtensionObject, fieldSchema: TSchema);

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col xl={6} md={12} className="field-editor-block">
        <Row middle="xs">
          <Col xl={4} md={3}>
            {fieldSchema.FieldDescription ? (
              <label htmlFor={this.id}>
                <Tooltip content={fieldSchema.FieldDescription} position={Position.TOP}>
                  <>
                    {fieldSchema.FieldTitle || fieldSchema.FieldName}:
                    {fieldSchema.IsRequired && <span className="pt-intent-danger"> *</span>}
                  </>
                </Tooltip>
              </label>
            ) : (
              <label htmlFor={this.id}>
                {fieldSchema.FieldTitle || fieldSchema.FieldName}:
                {fieldSchema.IsRequired && <span className="pt-intent-danger"> *</span>}
              </label>
            )}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
      </Col>
    );
  }
}

export class StringFieldEditor extends PlainFieldEditor<StringFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: StringFieldSchema) {
    return (
      <Col xl={8} md={6}>
        <InputText
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class NumericFieldEditor extends PlainFieldEditor<NumericFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: NumericFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <InputNumber
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          isInteger={fieldSchema.IsInteger}
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class BooleanFieldEditor extends PlainFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <CheckBox
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class DateFieldEditor extends PlainFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="date"
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class TimeFieldEditor extends PlainFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          type="time"
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class DateTimeFieldEditor extends PlainFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }
}

export class FileFieldEditor extends PlainFieldEditor<StringFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: StringFieldSchema) {
    return (
      <Col xl={8} md={6}>
        <div className="pt-input-group pt-fill">
          <InputText
            id={this.id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
            readOnly
          />
          <button className="pt-button pt-minimal pt-intent-warning pt-icon-cloud-download" />
        </div>
      </Col>
    );
  }
}

export class TextFieldEditor extends PlainFieldEditor<PlainFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={10} md={9}>
        <TextArea
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
        />
      </Col>
    );
  }

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <Col md={12} className="field-editor-block">
        <Row middle="xs">
          <Col xl={2} md={3}>
            {fieldSchema.FieldDescription ? (
              <label htmlFor={this.id}>
                <Tooltip content={fieldSchema.FieldDescription} position={Position.TOP}>
                  <>
                    {fieldSchema.FieldTitle || fieldSchema.FieldName}:
                    {fieldSchema.IsRequired && <span className="pt-intent-danger"> *</span>}
                  </>
                </Tooltip>
              </label>
            ) : (
              <label htmlFor={this.id}>
                {fieldSchema.FieldTitle || fieldSchema.FieldName}:
                {fieldSchema.IsRequired && <span className="pt-intent-danger"> *</span>}
              </label>
            )}
          </Col>
          {this.renderField(model, fieldSchema)}
        </Row>
      </Col>
    );
  }
}

@observer
export class ClassifierFieldEditor extends PlainFieldEditor<ClassifierFieldSchema> {
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
        />
      </Col>
    );
  }
}

export class EnumFieldEditor extends PlainFieldEditor<EnumFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: EnumFieldSchema) {
    const options = fieldSchema.Items.map(item => ({ value: item.Value, label: item.Alias }));
    return fieldSchema.ShowAsRadioButtons ? (
      <Col xl={8} md={9}>
        <RadioGroup
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          disabled={fieldSchema.IsReadOnly}
        />
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
        />
      </Col>
    );
  }
}

export class ExtensionFieldEditor extends PlainFieldEditor<ExtensionFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: ExtensionFieldSchema) {
    const options = Object.values(fieldSchema.Contents).map(contentSchema => ({
      value: contentSchema.ContentName,
      label: contentSchema.ContentTitle || contentSchema.ContentName
    }));
    const disabled =
      fieldSchema.IsReadOnly ||
      (!fieldSchema.Changeable && isArticleObject(model) && model.Timestamp instanceof Date);
    return (
      <Col xl={8} md={6}>
        <Select
          id={this.id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required={fieldSchema.IsRequired}
          disabled={disabled}
        />
      </Col>
    );
  }
}
