import React from "react";
import { Checkbox, ICheckboxProps } from "@blueprintjs/core";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class CheckBox extends AbstractControl<ICheckboxProps> {
  handleChange = action((e: any) => {
    const { model, name, onChange } = this.props;
    model[name] = !!e.target.checked;
    if (onChange) {
      onChange(e);
    }
  });

  render() {
    const { model, name, onChange, ...props } = this.props;
    return <Checkbox checked={!!model[name]} onChange={this.handleChange} {...props} />;
  }
}
