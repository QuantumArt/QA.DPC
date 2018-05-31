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
      <div className="form-check">
        <input
          type="checkbox"
          className={cn("form-check-input", className)}
          checked={!!model[name]}
          onChange={this.handleChange}
          {...props}
        />
      </div>
    );
  }
}
