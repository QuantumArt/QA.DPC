import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractControl } from "./AbstractControls";

@observer
export class InputSearch extends AbstractControl {
  handleChange = action((e: any) => {
    const { model, name } = this.props;
    model[name] = e.target.value;
  });

  render() {
    const { model, name, ...props } = this.props;
    const inputValue = model[name] != null ? model[name] : "";
    return (
      <input
        type="search"
        className="form-control"
        value={inputValue}
        onChange={this.handleChange}
        {...props}
      />
    );
  }
}
