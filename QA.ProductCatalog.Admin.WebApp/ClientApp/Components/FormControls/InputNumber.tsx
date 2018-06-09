import React from "react";
import { NumericInput, INumericInputProps } from "@blueprintjs/core";
import { action, runInAction } from "mobx";
import { observer } from "mobx-react";
import { ValidatableInput } from "./AbstractControls";

interface InputNumberProps extends INumericInputProps {
  isInteger?: boolean;
}

@observer
export class InputNumber extends ValidatableInput<InputNumberProps> {
  handleValueChange = (valueAsNumber: number, valueAsString: string) => {
    super.handleChange(valueAsNumber, valueAsString);
    const { model, name, isInteger } = this.props;
    const { hasFocus, editValue } = this.state;
    if (
      valueAsString === "" ||
      valueAsString === "-" ||
      (isInteger ? Number.isSafeInteger(valueAsNumber) : Number.isFinite(valueAsNumber))
    ) {
      if (hasFocus) {
        this.setState({ editValue: valueAsString });
      } else {
        runInAction(() => {
          model[name] = valueAsNumber;
        });
      }
    } else {
      this.setState({ editValue });
    }
  };

  @action
  handleBlur(e: any) {
    super.handleBlur(e);
    const { model, name } = this.props;
    const { editValue } = this.state;
    if (editValue === "") {
      model[name] = null;
    } else if (editValue !== "-") {
      model[name] = Number(editValue);
    }
  }

  render() {
    const {
      model,
      name,
      className,
      onFocus,
      onChange,
      onBlur,
      validate,
      isInteger,
      ...props
    } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <NumericInput
        value={inputValue}
        onFocus={this.handleFocus}
        onValueChange={this.handleValueChange}
        onBlur={this.handleBlur}
        fill
        {...props}
      />
    );
  }
}
