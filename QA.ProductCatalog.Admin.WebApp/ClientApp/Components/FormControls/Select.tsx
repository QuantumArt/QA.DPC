import React from "react";
import ReactSelect from "react-select";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

type SelectOption = { value: string; label: string };

@observer
export class Select extends AbstractControl<{
  options: SelectOption[];
}> {
  handleChange = action((selectedOption: SelectOption) => {
    const { model, name } = this.props;
    if (selectedOption) {
      model[name] = selectedOption.value;
    } else {
      model[name] = null;
    }
  });

  render() {
    const { model, name, options, ...props } = this.props;
    return (
      <ReactSelect value={model[name]} onChange={this.handleChange} options={options} {...props} />
    );
  }
}
