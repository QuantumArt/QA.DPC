import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import MaskedInput, { MaskedInputProps } from "react-text-mask";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputText extends AbstractInput<
  InputHTMLAttributes<HTMLInputElement> & MaskedInputProps
> {
  handleChange = e => {
    const { onChange } = this.props;
    if (onChange) {
      onChange(e);
    }
    this.setState({ editValue: e.target.value });
  };

  handleBlur = action((e: any) => {
    const { model, name, onBlur } = this.props;
    model[name] = this.state.editValue;
    if (onBlur) {
      onBlur(e);
    }
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, className, onChange, onFocus, onBlur, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return props.mask ? (
      <MaskedInput
        className={cn("pt-input pt-fill", className)}
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    ) : (
      <input
        type="text"
        className={cn("pt-input pt-fill", className)}
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
