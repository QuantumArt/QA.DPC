import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class InputSearch extends AbstractControl<InputHTMLAttributes<HTMLInputElement>> {
  handleChange = action((e: any) => {
    const { model, name, onChange } = this.props;
    model[name] = e.target.value;
    if (onChange) {
      onChange(e);
    }
  });

  render() {
    const { model, name, className, onChange, ...props } = this.props;
    const inputValue = model[name] != null ? model[name] : "";
    return (
      <input
        type="search"
        className={cn("form-control", className)}
        value={inputValue}
        onChange={this.handleChange}
        {...props}
      />
    );
  }
}
