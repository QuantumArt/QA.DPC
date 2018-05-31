import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

interface InputNumberProps extends InputHTMLAttributes<HTMLInputElement> {
  isInteger?: boolean;
}

@observer
export class InputNumber extends AbstractInput<InputNumberProps> {
  handleChange = e => {
    const editValue = e.target.value;
    const { onChange, isInteger } = this.props;
    if (onChange) {
      onChange(e);
    }
    if (
      editValue === "" ||
      editValue === "-" ||
      (isInteger ? Number.isSafeInteger(Number(editValue)) : Number.isFinite(Number(editValue)))
    ) {
      this.setState({ editValue });
    }
  };

  handleBlur = action((e: any) => {
    const { model, name, onBlur } = this.props;
    const { editValue } = this.state;
    if (editValue === "") {
      model[name] = null;
    } else if (editValue !== "-") {
      model[name] = Number(editValue);
    }
    if (onBlur) {
      onBlur(e);
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, className, onChange, onFocus, onBlur, isInteger, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <input
        type="text"
        className={cn("form-control", className)}
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
