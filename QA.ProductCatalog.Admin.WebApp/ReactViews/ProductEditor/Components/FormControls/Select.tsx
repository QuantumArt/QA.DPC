import "react-select/dist/react-select.css";
import React from "react";
import ReactSelect, { ReactSelectProps, Option } from "react-select";
import { action, isObservableArray } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";
import { isStateTreeNode, getIdentifier, getSnapshot } from "mobx-state-tree";

interface SelectProps extends ReactSelectProps {
  required?: boolean;
  multiple?: boolean;
}

@observer
export class Select extends ValidatableControl<SelectProps> {
  @action
  handleChange(selection: Option | Option[]) {
    super.handleChange(selection);
    const { model, name, required, clearable } = this.props;
    if (Array.isArray(selection)) {
      if (!required || clearable || selection.length > 0) {
        model[name] = selection.map(option => option.value);
      }
    } else if (selection) {
      model[name] = selection.value;
    } else if (!required || clearable) {
      model[name] = null;
    }
  }

  render() {
    const {
      model,
      name,
      onFocus,
      onChange,
      onBlur,
      required,
      multiple,
      placeholder = "",
      ...props
    } = this.props;
    let value = model[name];
    if ((multiple || props.multi) && isObservableArray(value)) {
      if (isStateTreeNode(value)) {
        // @ts-ignore
        value = getSnapshot(value);
      } else {
        value = value.peek();
      }
    } else {
      if (isStateTreeNode(value)) {
        value = getIdentifier(value);
      }
    }
    return (
      <ReactSelect
        value={value}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        placeholder={placeholder}
        clearable={!required}
        multi={multiple}
        {...props}
      />
    );
  }
}
