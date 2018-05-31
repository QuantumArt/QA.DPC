import React, { InputHTMLAttributes } from "react";
import cn from "classnames";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

interface RadioGroupProps extends InputHTMLAttributes<HTMLInputElement> {
  options: RadioOption[];
}

interface RadioOption {
  value: string;
  label: string;
  disabled?: boolean;
}

@observer
export class RadioGroup extends AbstractControl<RadioGroupProps> {
  handleChange = action((e: any) => {
    const { model, name, onChange, options } = this.props;
    const value = e.target.value;
    const selectedOption = options.find(option => option.value === value);
    if (selectedOption) {
      model[name] = selectedOption.value;
    } else {
      model[name] = null;
    }
    if (onChange) {
      onChange(e);
    }
  });

  render() {
    const { model, name, className, onChange, options, ...props } = this.props;
    const modelValue = model[name];
    return options.map(option => (
      <label key={option.value} className="editor-radio-group-option">
        <input
          type="radio"
          className={cn("form-check-input", className)}
          value={option.value}
          checked={option.value === modelValue}
          onChange={this.handleChange}
          disabled={!!option.disabled}
          {...props}
        />
        {option.label}
      </label>
    ));
  }
}
