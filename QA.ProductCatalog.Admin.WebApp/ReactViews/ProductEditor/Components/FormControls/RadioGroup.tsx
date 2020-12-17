import React from "react";
import { RadioGroup as PtRadioGroup, IOptionProps } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { ValidatableControl } from "./AbstractControls";

export interface RadioGroupProps {
  disabled?: boolean;
  inline?: boolean;
  label?: string;
  options?: IOptionProps[];
}

@observer
export class RadioGroup extends ValidatableControl<RadioGroupProps> {
  @action
  handleChange(e: any) {
    super.handleChange(e);
    const { model, name } = this.props;
    model[name] = e.target.value;
  }

  render() {
    const { model, name, onFocus, onChange, onBlur, ...props } = this.props;
    return <PtRadioGroup onChange={this.handleChange} selectedValue={model[name]} {...props} />;
  }
}
