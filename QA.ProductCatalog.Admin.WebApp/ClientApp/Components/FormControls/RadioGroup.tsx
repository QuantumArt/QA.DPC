import React from "react";
import { RadioGroup as PtRadioGroup, IOptionProps } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

export interface RadioGroupProps {
  disabled?: boolean;
  inline?: boolean;
  label?: string;
  options?: IOptionProps[];
}

@observer
export class RadioGroup extends AbstractControl<RadioGroupProps> {
  handleChange = action((e: any) => {
    const { model, name, onChange } = this.props;
    model[name] = e.target.value;
    if (onChange) {
      onChange(e);
    }
  });

  render() {
    const { model, name, onChange, ...props } = this.props;
    return <PtRadioGroup onChange={this.handleChange} selectedValue={model[name]} {...props} />;
  }
}
