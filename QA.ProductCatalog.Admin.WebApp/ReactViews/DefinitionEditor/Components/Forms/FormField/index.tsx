import React from "react";
import { observer } from "mobx-react-lite";
import { HTMLSelect, InputGroup, Switch, TextArea } from "@blueprintjs/core";
import { Field } from "react-final-form";
import { FormFieldType } from "DefinitionEditor/Enums";
import { ParsedModelType } from "Shared/Utils";
import "./Style.scss";
import cn from "classnames";

interface IProps {
  model?: ParsedModelType;
}

const FormField = observer(({ model }: IProps) => {
  const renderFieldDependsOnType = () => {
    const formFieldClassName = cn("form-field-element", {
      "form-field-element--inline": model.isInline,
      "form-field-element--hide": model.isHide
    });

    switch (model.type) {
      case FormFieldType.Text:
        return (
          <Field name={model.name} defaultValue={model.value}>
            {({ input }) => {
              return <InputGroup {...input} disabled={true} className={formFieldClassName} />;
            }}
          </Field>
        );
      case FormFieldType.Input:
        return (
          <Field
            name={model.name}
            value={model.value}
            defaultValue={model.value === null ? "" : model.value}
            parse={v => {
              return v;
            }}
            allowNull
          >
            {({ input }) => {
              return (
                <InputGroup
                  {...input}
                  className={formFieldClassName}
                  placeholder={model.placeholder || ""}
                />
              );
            }}
          </Field>
        );
      case FormFieldType.Textarea:
        return (
          <Field
            name={model.name}
            defaultValue={model.value === null ? "" : model.value}
            allowNull
            parse={v => {
              return v;
            }}
          >
            {({ input }) => {
              return <TextArea {...input} {...model.extraOptions} className={formFieldClassName} />;
            }}
          </Field>
        );
      case FormFieldType.Checkbox:
        return (
          <>
            <Field name={model.name} defaultValue={model.value} type={model.type}>
              {props => {
                return (
                  <div className={formFieldClassName}>
                    <Switch
                      label={model.subString ?? ""}
                      inline={true}
                      checked={props.input.checked}
                      name={props.input.name}
                      onChange={event => {
                        props.input.onChange(event);
                        model.onChangeCb && model.onChangeCb();
                      }}
                    />
                  </div>
                );
              }}
            </Field>
            {model.subModel && <FormField model={model.subModel} />}
          </>
        );
      case FormFieldType.Select:
        return (
          <Field name={model.name} defaultValue={model.value} type={model.type}>
            {props => {
              return (
                <HTMLSelect
                  name={props.input.name}
                  className={formFieldClassName}
                  iconProps={{ icon: "caret-down" }}
                  onChange={props.input.onChange}
                  value={props.input.value}
                  options={model.options}
                />
              );
            }}
          </Field>
        );
      default:
        return "";
    }
  };

  return model && <>{renderFieldDependsOnType()}</>;
});

export default FormField;
