import React from "react";
import { Input } from "reactstrap";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputNumber extends AbstractInput<{ isInteger?: boolean }> {
  handleChange = action(e => {
    // @ts-ignore
    const value = e.target.value;
    if (
      value === "" ||
      value === "-" ||
      (this.props.isInteger ? Number.isSafeInteger(Number(value)) : Number.isFinite(Number(value)))
    ) {
      this.editValue = value;
    }
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    this.hasFocus = false;
    if (this.editValue === "") {
      model[name] = null;
    } else if (this.editValue !== "-") {
      model[name] = Number(this.editValue);
    }
  });

  render() {
    const { model, name, isInteger, ...props } = this.props;
    const inputValue = this.hasFocus ? this.editValue : model[name] != null ? model[name] : "";
    return (
      <Input
        type="text"
        bsSize="sm"
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
