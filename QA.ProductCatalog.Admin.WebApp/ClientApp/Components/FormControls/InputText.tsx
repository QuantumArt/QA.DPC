import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import MaskedInput, { MaskedInputProps } from "react-text-mask";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableInput } from "./AbstractControls";

@observer
export class InputText extends ValidatableInput<
  InputHTMLAttributes<HTMLInputElement> & MaskedInputProps
> {
  handleChange(e: any) {
    super.handleChange(e);
    this.setState({ editValue: e.target.value });
  }

  @action
  handleBlur(e: any) {
    super.handleBlur(e);
    const { model, name } = this.props;
    model[name] = this.state.editValue;
  }

  render() {
    const { model, name, className, onFocus, onChange, onBlur, validate, ...props } = this.props;
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
