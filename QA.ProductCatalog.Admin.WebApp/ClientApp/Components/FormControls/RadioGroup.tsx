import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

type RadioOption = { value: string; label: string };

@observer
export class RadioGroup extends AbstractControl<{
  options: RadioOption[];
}> {
  handleChange = action((e: any) => {
    const { model, name, options } = this.props;
    const value = e.target.value;
    const selectedOption = options.find(option => option.value === value);
    if (selectedOption) {
      model[name] = selectedOption.value;
    } else {
      model[name] = null;
    }
  });

  render() {
    const { model, name, options, ...props } = this.props;
    const value = model[name];
    return options.map(option => (
      <label key={option.value} className="editor-radio-group-option">
        <input
          type="radio"
          className="form-check-input"
          value={option.value}
          checked={option.value === value}
          onChange={this.handleChange}
          {...props}
        />
        {option.label}
      </label>
    ));
  }
}
