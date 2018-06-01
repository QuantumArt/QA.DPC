import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class CheckBox extends AbstractControl<InputHTMLAttributes<HTMLInputElement>> {
  handleChange = action((e: any) => {
    const { model, name, onChange } = this.props;
    model[name] = !!e.target.checked;
    if (onChange) {
      onChange(e);
    }
  });

  render() {
    const { model, name, className, onChange, ...props } = this.props;
    return (
      <label className="custom-control custom-checkbox editor-checkbox">
        <input
          type="checkbox"
          className={cn("custom-control-input", className)}
          checked={!!model[name]}
          onChange={this.handleChange}
          {...props}
        />
        <span className="custom-control-label" />
      </label>
    );
  }
}
