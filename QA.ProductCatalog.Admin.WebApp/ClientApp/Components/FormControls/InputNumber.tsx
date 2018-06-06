import React from "react";
import { NumericInput, INumericInputProps } from "@blueprintjs/core";
import { action, runInAction } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

interface InputNumberProps extends INumericInputProps {
  isInteger?: boolean;
}

@observer
export class InputNumber extends AbstractInput<InputNumberProps> {
  handleValueChange = (valueAsNumber: number, valueAsString: string) => {
    const { model, name, onChange, isInteger } = this.props;
    const { hasFocus, editValue } = this.state;
    if (
      valueAsString === "" ||
      valueAsString === "-" ||
      (isInteger ? Number.isSafeInteger(valueAsNumber) : Number.isFinite(valueAsNumber))
    ) {
      if (onChange) {
        onChange(valueAsString);
      }
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
      <NumericInput
        value={inputValue}
        onFocus={this.handleFocus}
        onBlur={this.handleBlur}
        onValueChange={this.handleValueChange}
        fill
        {...props}
      />
    );
  }
}
