import React from "react";
import { Input } from "reactstrap";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputText extends AbstractInput {
  handleChange = action((e: any) => {
    this.editValue = e.target.value;
  });

  handleBlur = action(() => {
    const { model, name } = this.props;
    this.hasFocus = false;
    model[name] = this.editValue;
  });

  render() {
    const { model, name, ...props } = this.props;
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
