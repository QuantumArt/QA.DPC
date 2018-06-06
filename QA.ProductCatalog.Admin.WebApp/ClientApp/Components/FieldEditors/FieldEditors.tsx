import "./FieldEditors.scss";
import React, { Component } from "react";
import { Col } from "react-flexbox-grid";
import { Tooltip } from "@blueprintjs/core";
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
  _id = `_${Math.random()
    .toString(36)
    .slice(2)}`;

  render() {
    const { model, fieldSchema } = this.props;
    return (
      <>
        <Col className="field-editor-col" xl={2} lg={3} md={4}>
          <label htmlFor={this._id}>
            {/* <Tooltip content={fieldSchema.FieldDescription}> */}
            {fieldSchema.FieldTitle || fieldSchema.FieldName}:
            {fieldSchema.IsRequired && <span className="pt-intent-danger"> *</span>}
            {/* </Tooltip> */}
          </label>
        </Col>
        {this.renderField(model, fieldSchema)}
      </>
    );
  }

  abstract renderField(model: ArticleObject | ExtensionObject, fieldSchema: TSchema);
}

export class StringFieldEditor extends PlainFieldEditor<StringFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: StringFieldSchema) {
    return (
      <>
        <Col className="field-editor-col" xl={3} lg={5} md={7}>
          <InputText
            id={this._id}
            model={model}
            name={fieldSchema.FieldName}
            disabled={fieldSchema.IsReadOnly}
          />
        </Col>
        <Col className="field-editor-col" xl={1} lg={4} md={1} />
      </>
    );
  }
}

export class NumericFieldEditor extends PlainFieldEditor<NumericFieldSchema> {
  renderField(model: ArticleObject | ExtensionObject, fieldSchema: NumericFieldSchema) {
    return (
      <Col className="field-editor-col" xl={2} xlOffset={2} lg={3} lgOffset={6} md={4} mdOffset={4}>
        <InputNumber
          id={this._id}
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
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        <CheckBox
          id={this._id}
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
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        <DatePicker
          id={this._id}
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
      <Col className="field-editor-col" xl={2} xlOffset={2} lg={3} lgOffset={6} md={4} mdOffset={4}>
        <DatePicker
          id={this._id}
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
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        <DatePicker
          id={this._id}
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
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        <div className="pt-input-group pt-fill">
          <InputText
            id={this._id}
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
      <Col className="field-editor-col" xl={10} lg={9} md={8}>
        <TextArea
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={fieldSchema.IsReadOnly}
        />
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
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        <Select
          id={this._id}
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
    return (
      <Col className="field-editor-col" xl={4} lg={6} lgOffset={3} md={8}>
        {fieldSchema.ShowAsRadioButtons ? (
          <RadioGroup
            model={model}
            name={fieldSchema.FieldName}
            options={options}
            disabled={fieldSchema.IsReadOnly}
          />
        ) : (
          <Select
            id={this._id}
            model={model}
            name={fieldSchema.FieldName}
            options={options}
            required={fieldSchema.IsRequired}
            disabled={fieldSchema.IsReadOnly}
          />
        )}
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
      <Col className="field-editor-col" xl={4} xlOffset={6} lg={6} lgOffset={3} md={8}>
        <Select
          id={this._id}
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
