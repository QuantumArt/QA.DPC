import React from "react";
import { action } from "mobx";
import { observer } from "mobx-react";
import { AbstractInput } from "./AbstractControls";

@observer
export class InputText extends AbstractInput {
  handleChange = e => {
    this.setState({ editValue: e.target.value });
  };

  handleBlur = action(() => {
    const { model, name } = this.props;
    model[name] = this.state.editValue;
    this.setState({ hasFocus: false });
  });

  render() {
    const { model, name, ...props } = this.props;
    const { hasFocus, editValue } = this.state;
    const inputValue = hasFocus ? editValue : model[name] != null ? model[name] : "";
    return (
      <input
        type="text"
        className="form-control form-control-sm"
        value={inputValue}
        onFocus={this.handleFocus}
        onChange={this.handleChange}
        onBlur={this.handleBlur}
        {...props}
      />
    );
  }
}
