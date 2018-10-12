import React from "react";
import { Col } from "react-flexbox-grid";
import cn from "classnames";
import { observer } from "mobx-react";
import { ArticleObject } from "Models/EditorDataModels";
import {
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
  CheckBox,
  DatePicker
} from "Components/FormControls/FormControls";
import { pattern } from "Utils/Validators";
import { Intent } from "@blueprintjs/core";
import { AbstractFieldEditor } from "./AbstractFieldEditor";
import { Validate } from "mst-validation-mixin";
export { FileFieldEditor } from "./FileFieldEditor";
export { TextFieldEditor } from "./TextFieldEditor";
export { RelationFieldCheckList } from "./RelationFieldCheckList/RelationFieldCheckList";
export { RelationFieldSelect } from "./RelationFieldSelect/RelationFieldSelect";
export { RelationFieldForm } from "./RelationFieldForm/RelationFieldForm";
export { RelationFieldTabs } from "./RelationFieldTabs/RelationFieldTabs";
export { RelationFieldAccordion } from "./RelationFieldAccordion/RelationFieldAccordion";
export {
  SingleRelationFieldTags,
  MultiRelationFieldTags,
  RelationFieldTags
} from "./RelationFieldTags/RelationFieldTags";
export {
  SingleRelationFieldTable,
  MultiRelationFieldTable,
  RelationFieldTable
} from "./RelationFieldTable/RelationFieldTable";

@observer
export class StringFieldEditor extends AbstractFieldEditor {
  renderValidation(model: ArticleObject, fieldSchema: StringFieldSchema) {
    return (
      <>
        <Validate
          model={model}
          name={fieldSchema.FieldName}
          silent
          rules={fieldSchema.RegexPattern && pattern(fieldSchema.RegexPattern)}
        />
        {super.renderValidation(model, fieldSchema)}
      </>
    );
  }

  renderField(model: ArticleObject, fieldSchema: StringFieldSchema) {
    return (
      <Col xl md={6}>
        <InputText
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class NumericFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: NumericFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <InputNumber
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          isInteger={fieldSchema.IsInteger}
          disabled={this._readonly}
          intent={
            model.hasVisibleErrors(fieldSchema.FieldName)
              ? Intent.DANGER
              : model.isEdited(fieldSchema.FieldName)
                ? Intent.PRIMARY
                : Intent.NONE
          }
        />
      </Col>
    );
  }
}

@observer
export class BooleanFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col md>
        <CheckBox
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class DateFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          type="date"
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class TimeFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          type="time"
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class DateTimeFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: PlainFieldSchema) {
    return (
      <Col xl={4} md={3}>
        <DatePicker
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class ClassifierFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: ClassifierFieldSchema) {
    const value = model[fieldSchema.FieldName];
    const options = value ? [{ value, label: value }] : [];
    return (
      <Col xl md={6}>
        <Select
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required
          disabled
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class EnumFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: EnumFieldSchema) {
    const options = fieldSchema.Items.map(item => ({ value: item.Value, label: item.Alias }));
    return fieldSchema.ShowAsRadioButtons ? (
      <Col md>
        <RadioGroup
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    ) : (
      <Col xl md={6}>
        <Select
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required={fieldSchema.IsRequired}
          disabled={this._readonly}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}

@observer
export class ExtensionFieldEditor extends AbstractFieldEditor {
  renderField(model: ArticleObject, fieldSchema: ExtensionFieldSchema) {
    const options = Object.values(fieldSchema.ExtensionContents).map(contentSchema => ({
      value: contentSchema.ContentName,
      label: contentSchema.ContentTitle || contentSchema.ContentName
    }));

    const disabled = this._readonly || (!fieldSchema.Changeable && model._ServerId > 0);
    return (
      <Col xl md={6}>
        <Select
          id={this._id}
          model={model}
          name={fieldSchema.FieldName}
          options={options}
          required={fieldSchema.IsRequired}
          disabled={disabled}
          className={cn({
            "bp3-intent-primary": model.isEdited(fieldSchema.FieldName),
            "bp3-intent-danger": model.hasVisibleErrors(fieldSchema.FieldName)
          })}
        />
      </Col>
    );
  }
}
