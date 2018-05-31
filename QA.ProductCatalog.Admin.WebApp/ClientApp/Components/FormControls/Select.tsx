import React from "react";
import ReactSelect, { ReactSelectProps, Option } from "react-select";
import { action, isObservableArray } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

interface SelectProps extends ReactSelectProps {
  required?: boolean;
  multiple?: boolean;
}

@observer
export class Select extends AbstractControl<SelectProps> {
  handleChange = action((selection: Option | Option[]) => {
    const { model, name, onChange, required, clearable } = this.props;
    if (Array.isArray(selection)) {
      if (!required || clearable || selection.length > 0) {
        model[name] = selection.map(option => option.value);
      }
    } else if (selection) {
      model[name] = selection.value;
    } else if (!required || clearable) {
      model[name] = null;
    }
    if (onChange) {
      onChange(selection);
    }
  });

  render() {
    const { model, name, onChange, required, multiple, ...props } = this.props;
    let value = model[name];
    if ((multiple || props.multi) && isObservableArray(value)) {
      value = value.slice();
    }
    return (
      <ReactSelect
        value={value}
        onChange={this.handleChange}
        clearable={!required}
        multi={multiple}
        {...props}
      />
    );
  }
}
