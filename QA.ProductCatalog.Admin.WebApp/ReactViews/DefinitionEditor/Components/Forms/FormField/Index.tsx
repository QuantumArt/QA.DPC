import React from "react";
import { observer } from "mobx-react-lite";
import { HTMLSelect, InputGroup, Switch, TextArea } from "@blueprintjs/core";
import { Field } from "react-final-form";
import { FormFieldType } from "DefinitionEditor/Enums";
import "./Style.scss";
import { ParsedModelType } from "Shared/Utils";

interface Props {
  model: ParsedModelType;
}

const FormField = observer<Props>(({ model }) => {
  const renderFieldDependsOnType = () => {
    switch (model.type) {
      case FormFieldType.Text:
        return (
          <div className="form-field">
            <label className="form-field__label">{model.label}</label>
            <Field name={model.label} defaultValue={model.value}>
              {({ input }) => {
                return <InputGroup {...input} disabled={true} className="form-field__element" />;
              }}
            </Field>
          </div>
        );
      case FormFieldType.Input:
        return (
          <div className="form-field">
            <label className="form-field__label">{model.label}</label>
            <Field name={model.label} defaultValue={model.value}>
              {({ input }) => {
                return (
                  <InputGroup
                    {...input}
                    className="form-field__element"
                    placeholder={model.placeholder || ""}
                  />
                );
              }}
            </Field>
          </div>
        );
      case FormFieldType.Textarea:
        return (
          <div className="form-field">
            <label className="form-field__label">{model.label}</label>
            <Field name={model.label} defaultValue={model.value}>
              {({ input }) => {
                return <TextArea {...input} {...model.extraOptions} className="field-element" />;
              }}
            </Field>
          </div>
        );
      case FormFieldType.Checkbox:
        return (
          <div className="form-field">
            <label className="form-field__label">{model.label}</label>
            <Field name={model.label} initialValue={model.value} type={model.type}>
              {props => {
                return (
                  <div className="form-field__element">
                    <Switch
                      label={model.subString ?? ""}
                      inline={true}
                      checked={props.input.checked}
                      name={props.input.name}
                      onChange={event => {
                        props.input.onChange(event);
                      }}
                    />
                  </div>
                );
              }}
            </Field>
          </div>
        );
      case FormFieldType.Select:
        return (
          <div className="form-field">
            <label className="form-field__label">{model.label}</label>
            <Field name={model.label} defaultValue={model.value} type={model.type}>
              {props => {
                return (
                  <HTMLSelect
                    name={props.input.name}
                    className="form-field__element"
                    iconProps={{ icon: "caret-down" }}
                    onChange={props.input.onChange}
                    value={props.input.value}
                    options={model.getParsedOptions()}
                  />
                );
              }}
            </Field>
          </div>
        );
      default:
        return "";
    }
  };

  return model && <>{renderFieldDependsOnType()}</>;
});

export default FormField;
