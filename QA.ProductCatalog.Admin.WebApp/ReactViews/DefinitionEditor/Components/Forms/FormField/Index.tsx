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

//TODO добавить в модель name и юзать его как ключ. Т.к. label может отсутствовать.
const FormField = observer(({ model }: IProps) => {
  const renderFieldDependsOnType = React.useMemo(() => {
    const formFieldClassName = cn("form-field-element", {
      "form-field-element--inline": model.isInline,
      "form-field-element--hide": model.isHide
    });

    switch (model.type) {
      case FormFieldType.Text:
        return (
          <Field name={model.label} defaultValue={model.value}>
            {({ input }) => {
              return <InputGroup {...input} disabled={true} className={formFieldClassName} />;
            }}
          </Field>
        );
      case FormFieldType.Input:
        return (
          <Field name={model.label} defaultValue={model.value} value={model.value}>
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
          <Field name={model.label} defaultValue={model.value}>
            {({ input }) => {
              return <TextArea {...input} {...model.extraOptions} className={formFieldClassName} />;
            }}
          </Field>
        );
      case FormFieldType.Checkbox:
        return (
          <>
            <Field name={model.label} defaultValue={model.value} type={model.type}>
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
                        model?.toggleValue();
                      }}
                    />
                  </div>
                );
              }}
            </Field>
            {model.subComponentOnCheck && <FormField model={model.subComponentOnCheck} />}
          </>
        );
      case FormFieldType.Select:
        return (
          <Field name={model.label} defaultValue={model.value} type={model.type}>
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
  }, [model.isHide, model.isInline, model.label, model.value, model.type]);

  return model && <>{renderFieldDependsOnType}</>;
});

export default FormField;
