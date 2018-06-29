import React from "react";
import { Checkbox, ICheckboxProps } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";

@observer
export class CheckBox extends ValidatableControl<ICheckboxProps> {
  @action
  handleChange(e: any) {
    super.handleChange(e);
    const { model, name } = this.props;
    model[name] = !!e.target.checked;
  }

  render() {
    const { model, name, onFocus, onChange, onBlur, validate, ...props } = this.props;
    return (
      <Checkbox
        checked={!!model[name]}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
